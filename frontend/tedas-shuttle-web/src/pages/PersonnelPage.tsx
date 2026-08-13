import Add from '@mui/icons-material/Add'
import Edit from '@mui/icons-material/Edit'
import {
  Alert,
  Box,
  Button,
  Chip,
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
} from '@mui/material'
import { useMemo, useState } from 'react'
import { PageHeader } from '../components/PageHeader'
import { PersonnelFormDialog } from '../features/personnel/PersonnelFormDialog'
import type { PersonnelFormValues, PersonnelListItem } from '../features/personnel/personnelTypes'
import {
  useCreatePersonnel,
  usePersonnel,
  useUpdatePersonnel,
  useUpdatePersonnelStatus,
} from '../features/personnel/usePersonnel'
import { getApiErrorMessage } from '../utils/apiErrors'

const pageSizeOptions = [10, 25, 50]

export function PersonnelPage() {
  const [page, setPage] = useState(0)
  const [pageSize, setPageSize] = useState(10)
  const [search, setSearch] = useState('')
  const [isActiveFilter, setIsActiveFilter] = useState<'all' | 'active' | 'inactive'>('all')
  const [dialogOpen, setDialogOpen] = useState(false)
  const [selectedPersonnel, setSelectedPersonnel] = useState<PersonnelListItem | null>(null)

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

  const personnelQuery = usePersonnel(query)
  const createPersonnelMutation = useCreatePersonnel()
  const updatePersonnelMutation = useUpdatePersonnel()
  const updateStatusMutation = useUpdatePersonnelStatus()

  const mutationError =
    createPersonnelMutation.error ??
    updatePersonnelMutation.error ??
    updateStatusMutation.error

  function openCreateDialog() {
    setSelectedPersonnel(null)
    setDialogOpen(true)
  }

  function openEditDialog(personnel: PersonnelListItem) {
    setSelectedPersonnel(personnel)
    setDialogOpen(true)
  }

  function closeDialog() {
    setDialogOpen(false)
    createPersonnelMutation.reset()
    updatePersonnelMutation.reset()
  }

  function handleSubmit(values: PersonnelFormValues) {
    if (selectedPersonnel) {
      updatePersonnelMutation.mutate(
        {
          id: selectedPersonnel.id,
          values: {
            firstName: values.firstName,
            lastName: values.lastName,
            department: values.department,
            title: values.title,
            phone: values.phone,
            email: values.email,
            address: values.address,
            latitude: values.latitude,
            longitude: values.longitude,
          },
        },
        { onSuccess: closeDialog },
      )
      return
    }

    createPersonnelMutation.mutate(values, { onSuccess: closeDialog })
  }

  return (
    <Stack spacing={3}>
      <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'flex-start', gap: 2 }}>
        <PageHeader title="Personeller" />
        <Button startIcon={<Add />} variant="contained" onClick={openCreateDialog}>
          Personel Ekle
        </Button>
      </Stack>

      <Paper variant="outlined" sx={{ borderRadius: 1, p: 2 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
          <TextField
            label="Sicil, ad veya soyad ara"
            size="small"
            value={search}
            onChange={(event) => {
              setSearch(event.target.value)
              setPage(0)
            }}
            sx={{ width: { xs: '100%', md: 360 } }}
          />
          <FormControl size="small" sx={{ width: { xs: '100%', md: 220 } }}>
            <InputLabel id="personnel-status-filter-label">Durum</InputLabel>
            <Select
              labelId="personnel-status-filter-label"
              label="Durum"
              value={isActiveFilter}
              onChange={(event) => {
                setIsActiveFilter(event.target.value as typeof isActiveFilter)
                setPage(0)
              }}
            >
              <MenuItem value="all">Tümü</MenuItem>
              <MenuItem value="active">Aktif</MenuItem>
              <MenuItem value="inactive">Pasif</MenuItem>
            </Select>
          </FormControl>
        </Stack>
      </Paper>

      {personnelQuery.isError ? (
        <Alert severity="error">{getApiErrorMessage(personnelQuery.error)}</Alert>
      ) : null}
      {mutationError ? <Alert severity="error">{getApiErrorMessage(mutationError)}</Alert> : null}

      <TableContainer component={Paper} variant="outlined" sx={{ borderRadius: 1 }}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Sicil</TableCell>
              <TableCell>Ad Soyad</TableCell>
              <TableCell>Departman</TableCell>
              <TableCell>Unvan</TableCell>
              <TableCell>İletişim</TableCell>
              <TableCell>Durum</TableCell>
              <TableCell align="right">İşlemler</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {personnelQuery.data?.items.map((personnel) => (
              <TableRow key={personnel.id} hover>
                <TableCell>{personnel.registrationNumber}</TableCell>
                <TableCell>{personnel.fullName}</TableCell>
                <TableCell>{personnel.department ?? '-'}</TableCell>
                <TableCell>{personnel.title ?? '-'}</TableCell>
                <TableCell>
                  <Stack spacing={0.25}>
                    <Box component="span">{personnel.phone ?? '-'}</Box>
                    <Box component="span" sx={{ color: 'text.secondary', fontSize: 13 }}>
                      {personnel.email ?? '-'}
                    </Box>
                  </Stack>
                </TableCell>
                <TableCell>
                  <Chip
                    label={personnel.isActive ? 'Aktif' : 'Pasif'}
                    color={personnel.isActive ? 'success' : 'default'}
                    size="small"
                    variant="outlined"
                  />
                </TableCell>
                <TableCell align="right">
                  <Tooltip title="Düzenle">
                    <IconButton size="small" onClick={() => openEditDialog(personnel)}>
                      <Edit fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title={personnel.isActive ? 'Pasif yap' : 'Aktif yap'}>
                    <Switch
                      size="small"
                      checked={personnel.isActive}
                      disabled={updateStatusMutation.isPending}
                      onChange={(event) =>
                        updateStatusMutation.mutate({
                          id: personnel.id,
                          isActive: event.target.checked,
                        })
                      }
                    />
                  </Tooltip>
                </TableCell>
              </TableRow>
            ))}
            {personnelQuery.data?.items.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} align="center" sx={{ py: 6, color: 'text.secondary' }}>
                  Personel kaydı bulunamadı.
                </TableCell>
              </TableRow>
            ) : null}
          </TableBody>
        </Table>
        <TablePagination
          component="div"
          count={personnelQuery.data?.totalCount ?? 0}
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

      <PersonnelFormDialog
        open={dialogOpen}
        mode={selectedPersonnel ? 'edit' : 'create'}
        personnel={selectedPersonnel}
        isSubmitting={createPersonnelMutation.isPending || updatePersonnelMutation.isPending}
        errorMessage={
          createPersonnelMutation.error
            ? getApiErrorMessage(createPersonnelMutation.error)
            : updatePersonnelMutation.error
              ? getApiErrorMessage(updatePersonnelMutation.error)
              : null
        }
        onClose={closeDialog}
        onSubmit={handleSubmit}
      />
    </Stack>
  )
}
