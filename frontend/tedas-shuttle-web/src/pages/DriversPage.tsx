import Add from '@mui/icons-material/Add'
import Edit from '@mui/icons-material/Edit'
import LinkIcon from '@mui/icons-material/Link'
import {
  Alert,
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  IconButton,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Stack,
  Switch,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material'
import { useMemo, useState } from 'react'
import { PageHeader } from '../components/PageHeader'
import { DriverFormDialog } from '../features/drivers/DriverFormDialog'
import type { DriverFormValues, DriverListItem } from '../features/drivers/driverTypes'
import {
  useCreateDriver,
  useDrivers,
  useUpdateDriver,
  useUpdateDriverShiftAssignment,
  useUpdateDriverStatus,
} from '../features/drivers/useDrivers'
import { shiftTypes } from '../features/shifts/shiftTypes'
import { useAllShifts } from '../features/shifts/useShifts'
import { getApiErrorMessage } from '../utils/apiErrors'

const pageSizeOptions = [10, 25, 50]

export function DriversPage() {
  const [page, setPage] = useState(0)
  const [pageSize, setPageSize] = useState(10)
  const [search, setSearch] = useState('')
  const [isActiveFilter, setIsActiveFilter] = useState<'all' | 'active' | 'inactive'>('all')
  const [dialogOpen, setDialogOpen] = useState(false)
  const [assignmentDialogOpen, setAssignmentDialogOpen] = useState(false)
  const [selectedDriver, setSelectedDriver] = useState<DriverListItem | null>(null)
  const [selectedShiftId, setSelectedShiftId] = useState('')

  const query = useMemo(
    () => ({
      page: page + 1,
      pageSize,
      search: search.trim() || undefined,
      isActive:
        isActiveFilter === 'all'
          ? undefined
          : isActiveFilter === 'active',
    }),
    [isActiveFilter, page, pageSize, search],
  )

  const driversQuery = useDrivers(query)
  const shiftsQuery = useAllShifts(true)
  const createDriverMutation = useCreateDriver()
  const updateDriverMutation = useUpdateDriver()
  const updateStatusMutation = useUpdateDriverStatus()
  const updateAssignmentMutation = useUpdateDriverShiftAssignment()

  const mutationError =
    createDriverMutation.error ??
    updateDriverMutation.error ??
    updateStatusMutation.error ??
    updateAssignmentMutation.error

  function openCreateDialog() {
    setSelectedDriver(null)
    setDialogOpen(true)
  }

  function openEditDialog(driver: DriverListItem) {
    setSelectedDriver(driver)
    setDialogOpen(true)
  }

  function closeDialog() {
    setDialogOpen(false)
    createDriverMutation.reset()
    updateDriverMutation.reset()
  }

  function openAssignmentDialog(driver: DriverListItem) {
    setSelectedDriver(driver)
    setSelectedShiftId(driver.assignedShift?.shuttleShiftId ?? '')
    setAssignmentDialogOpen(true)
  }

  function closeAssignmentDialog() {
    setAssignmentDialogOpen(false)
    updateAssignmentMutation.reset()
  }

  function handleSubmit(values: DriverFormValues) {
    if (selectedDriver) {
      updateDriverMutation.mutate(
        {
          id: selectedDriver.id,
          values,
        },
        { onSuccess: closeDialog },
      )
      return
    }

    createDriverMutation.mutate(values, { onSuccess: closeDialog })
  }

  function handleAssignmentSubmit() {
    if (!selectedDriver) {
      return
    }

    updateAssignmentMutation.mutate(
      {
        id: selectedDriver.id,
        shuttleShiftId: selectedShiftId || null,
      },
      { onSuccess: closeAssignmentDialog },
    )
  }

  return (
    <Stack spacing={3}>
      <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'flex-start', gap: 2 }}>
        <PageHeader title="Soforler" />
        <Button startIcon={<Add />} variant="contained" onClick={openCreateDialog}>
          Sofor Ekle
        </Button>
      </Stack>

      <Paper variant="outlined" sx={{ borderRadius: 1, p: 2 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
          <TextField
            label="Ad, telefon veya ehliyet ara"
            size="small"
            value={search}
            onChange={(event) => {
              setSearch(event.target.value)
              setPage(0)
            }}
            sx={{ width: { xs: '100%', md: 360 } }}
          />
          <FormControl size="small" sx={{ width: { xs: '100%', md: 220 } }}>
            <InputLabel id="driver-status-filter-label">Durum</InputLabel>
            <Select
              labelId="driver-status-filter-label"
              label="Durum"
              value={isActiveFilter}
              onChange={(event) => {
                setIsActiveFilter(event.target.value as typeof isActiveFilter)
                setPage(0)
              }}
            >
              <MenuItem value="all">Tumu</MenuItem>
              <MenuItem value="active">Aktif</MenuItem>
              <MenuItem value="inactive">Pasif</MenuItem>
            </Select>
          </FormControl>
        </Stack>
      </Paper>

      {driversQuery.isError ? (
        <Alert severity="error">{getApiErrorMessage(driversQuery.error)}</Alert>
      ) : null}
      {shiftsQuery.isError ? <Alert severity="error">{getApiErrorMessage(shiftsQuery.error)}</Alert> : null}
      {mutationError ? <Alert severity="error">{getApiErrorMessage(mutationError)}</Alert> : null}

      <TableContainer component={Paper} variant="outlined" sx={{ borderRadius: 1 }}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Ad Soyad</TableCell>
              <TableCell>Telefon</TableCell>
              <TableCell>Ehliyet No</TableCell>
              <TableCell>Vardiya</TableCell>
              <TableCell>Durum</TableCell>
              <TableCell align="right">Islemler</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {driversQuery.data?.items.map((driver) => (
              <TableRow key={driver.id} hover>
                <TableCell>{driver.fullName}</TableCell>
                <TableCell>{driver.phone ?? '-'}</TableCell>
                <TableCell>{driver.licenseNumber}</TableCell>
                <TableCell>
                  {driver.assignedShift ? (
                    <Stack spacing={0.25}>
                      <Box component="span">
                        {driver.assignedShift.physicalShuttleCode} / {driver.assignedShift.shiftName}
                      </Box>
                      <Box component="span" sx={{ color: 'text.secondary', fontSize: 13 }}>
                        {getShiftTypeLabel(driver.assignedShift.shiftType)}
                      </Box>
                    </Stack>
                  ) : (
                    '-'
                  )}
                </TableCell>
                <TableCell>
                  <Chip
                    label={driver.isActive ? 'Aktif' : 'Pasif'}
                    color={driver.isActive ? 'success' : 'default'}
                    size="small"
                    variant="outlined"
                  />
                </TableCell>
                <TableCell align="right">
                  <Tooltip title="Vardiya iliskisi">
                    <IconButton size="small" onClick={() => openAssignmentDialog(driver)}>
                      <LinkIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title="Duzenle">
                    <IconButton size="small" onClick={() => openEditDialog(driver)}>
                      <Edit fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title={driver.isActive ? 'Pasif yap' : 'Aktif yap'}>
                    <Switch
                      size="small"
                      checked={driver.isActive}
                      disabled={updateStatusMutation.isPending}
                      onChange={(event) =>
                        updateStatusMutation.mutate({
                          id: driver.id,
                          isActive: event.target.checked,
                        })
                      }
                    />
                  </Tooltip>
                </TableCell>
              </TableRow>
            ))}
            {driversQuery.data?.items.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} align="center" sx={{ py: 6, color: 'text.secondary' }}>
                  Sofor kaydi bulunamadi.
                </TableCell>
              </TableRow>
            ) : null}
          </TableBody>
        </Table>
        <TablePagination
          component="div"
          count={driversQuery.data?.totalCount ?? 0}
          page={page}
          rowsPerPage={pageSize}
          rowsPerPageOptions={pageSizeOptions}
          labelRowsPerPage="Sayfa boyutu"
          onPageChange={(_, nextPage) => setPage(nextPage)}
          onRowsPerPageChange={(event) => {
            setPageSize(Number(event.target.value))
            setPage(0)
          }}
        />
      </TableContainer>

      <DriverFormDialog
        open={dialogOpen}
        mode={selectedDriver ? 'edit' : 'create'}
        driver={selectedDriver}
        isSubmitting={createDriverMutation.isPending || updateDriverMutation.isPending}
        errorMessage={
          createDriverMutation.error
            ? getApiErrorMessage(createDriverMutation.error)
            : updateDriverMutation.error
              ? getApiErrorMessage(updateDriverMutation.error)
              : null
        }
        onClose={closeDialog}
        onSubmit={handleSubmit}
      />

      <Dialog open={assignmentDialogOpen} onClose={closeAssignmentDialog} maxWidth="sm" fullWidth>
        <DialogTitle>Vardiya Iliskisi</DialogTitle>
        <DialogContent>
          {updateAssignmentMutation.error ? (
            <Alert severity="error" sx={{ mb: 2 }}>
              {getApiErrorMessage(updateAssignmentMutation.error)}
            </Alert>
          ) : null}
          <Stack spacing={2} sx={{ pt: 1 }}>
            <Typography variant="body2" color="text.secondary">
              {selectedDriver?.fullName ?? '-'}
            </Typography>
            <FormControl size="small" fullWidth>
              <InputLabel id="driver-shift-assignment-label">Vardiya</InputLabel>
              <Select
                labelId="driver-shift-assignment-label"
                label="Vardiya"
                value={selectedShiftId}
                onChange={(event) => setSelectedShiftId(event.target.value)}
              >
                <MenuItem value="">Iliski yok</MenuItem>
                {shiftsQuery.data?.map((shift) => (
                  <MenuItem key={shift.id} value={shift.id}>
                    {shift.physicalShuttleCode} / {shift.name} ({getShiftTypeLabel(shift.shiftType)})
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={closeAssignmentDialog}>Vazgec</Button>
          <Button
            variant="contained"
            disabled={updateAssignmentMutation.isPending}
            onClick={handleAssignmentSubmit}
          >
            Kaydet
          </Button>
        </DialogActions>
      </Dialog>
    </Stack>
  )
}

function getShiftTypeLabel(value: number) {
  return shiftTypes.find((type) => type.value === value)?.label ?? '-'
}

