import Add from '@mui/icons-material/Add'
import Delete from '@mui/icons-material/Delete'
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
import { AssignmentFormDialog } from '../features/assignments/AssignmentFormDialog'
import type { AssignmentFormValues } from '../features/assignments/assignmentTypes'
import {
  useAssignments,
  useCreateAssignment,
  useDeactivateAssignment,
} from '../features/assignments/useAssignments'
import { usePersonnel } from '../features/personnel/usePersonnel'
import { useAllShifts } from '../features/shifts/useShifts'
import { getApiErrorMessage } from '../utils/apiErrors'

const pageSizeOptions = [10, 25, 50]

export function AssignmentsPage() {
  const [page, setPage] = useState(0)
  const [pageSize, setPageSize] = useState(10)
  const [search, setSearch] = useState('')
  const [isActiveFilter, setIsActiveFilter] = useState<'active' | 'inactive' | 'all'>('active')
  const [dialogOpen, setDialogOpen] = useState(false)

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

  const assignmentsQuery = useAssignments(query)
  const personnelQuery = usePersonnel({ page: 1, pageSize: 100, isActive: true })
  const shiftsQuery = useAllShifts(true)
  const createAssignmentMutation = useCreateAssignment()
  const deactivateAssignmentMutation = useDeactivateAssignment()

  const mutationError = createAssignmentMutation.error ?? deactivateAssignmentMutation.error

  function closeDialog() {
    setDialogOpen(false)
    createAssignmentMutation.reset()
  }

  function handleSubmit(values: AssignmentFormValues) {
    createAssignmentMutation.mutate(values, { onSuccess: closeDialog })
  }

  return (
    <Stack spacing={3}>
      <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'flex-start', gap: 2 }}>
        <PageHeader title="Servis Atamalari" />
        <Button startIcon={<Add />} variant="contained" onClick={() => setDialogOpen(true)}>
          Atama Baslat
        </Button>
      </Stack>

      <Paper variant="outlined" sx={{ borderRadius: 1, p: 2 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
          <TextField
            label="Personel veya servis ara"
            size="small"
            value={search}
            onChange={(event) => {
              setSearch(event.target.value)
              setPage(0)
            }}
            sx={{ width: { xs: '100%', md: 360 } }}
          />
          <FormControl size="small" sx={{ width: { xs: '100%', md: 220 } }}>
            <InputLabel id="assignment-status-filter-label">Durum</InputLabel>
            <Select
              labelId="assignment-status-filter-label"
              label="Durum"
              value={isActiveFilter}
              onChange={(event) => {
                setIsActiveFilter(event.target.value as typeof isActiveFilter)
                setPage(0)
              }}
            >
              <MenuItem value="active">Aktif</MenuItem>
              <MenuItem value="inactive">Pasif</MenuItem>
              <MenuItem value="all">Tumu</MenuItem>
            </Select>
          </FormControl>
        </Stack>
      </Paper>

      {assignmentsQuery.isError ? (
        <Alert severity="error">{getApiErrorMessage(assignmentsQuery.error)}</Alert>
      ) : null}
      {personnelQuery.isError ? <Alert severity="error">{getApiErrorMessage(personnelQuery.error)}</Alert> : null}
      {shiftsQuery.isError ? <Alert severity="error">{getApiErrorMessage(shiftsQuery.error)}</Alert> : null}
      {mutationError ? <Alert severity="error">{getApiErrorMessage(mutationError)}</Alert> : null}

      <TableContainer component={Paper} variant="outlined" sx={{ borderRadius: 1 }}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Personel</TableCell>
              <TableCell>Departman</TableCell>
              <TableCell>Servis / Vardiya</TableCell>
              <TableCell align="right">Doluluk</TableCell>
              <TableCell>Durum</TableCell>
              <TableCell>Atama Tarihi</TableCell>
              <TableCell align="right">Islemler</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {assignmentsQuery.data?.items.map((assignment) => (
              <TableRow key={assignment.id} hover>
                <TableCell>
                  <Stack spacing={0.25}>
                    <Box component="span">{assignment.personnelFullName}</Box>
                    <Box component="span" sx={{ color: 'text.secondary', fontSize: 13 }}>
                      {assignment.registrationNumber}
                    </Box>
                  </Stack>
                </TableCell>
                <TableCell>{assignment.department ?? '-'}</TableCell>
                <TableCell>
                  {assignment.physicalShuttleCode} / {assignment.shiftName}
                </TableCell>
                <TableCell align="right">
                  {assignment.occupancy} / {assignment.capacity}
                </TableCell>
                <TableCell>
                  <Chip
                    label={assignment.isActive ? 'Aktif' : 'Pasif'}
                    color={assignment.isActive ? 'success' : 'default'}
                    size="small"
                    variant="outlined"
                  />
                </TableCell>
                <TableCell>{new Date(assignment.assignedAt).toLocaleDateString('tr-TR')}</TableCell>
                <TableCell align="right">
                  <Tooltip title="Atamayi pasife al">
                    <span>
                      <IconButton
                        size="small"
                        disabled={!assignment.isActive || deactivateAssignmentMutation.isPending}
                        onClick={() => deactivateAssignmentMutation.mutate(assignment.id)}
                      >
                        <Delete fontSize="small" />
                      </IconButton>
                    </span>
                  </Tooltip>
                </TableCell>
              </TableRow>
            ))}
            {assignmentsQuery.data?.items.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} align="center" sx={{ py: 6, color: 'text.secondary' }}>
                  Atama kaydi bulunamadi.
                </TableCell>
              </TableRow>
            ) : null}
          </TableBody>
        </Table>
        <TablePagination
          component="div"
          count={assignmentsQuery.data?.totalCount ?? 0}
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

      <AssignmentFormDialog
        open={dialogOpen}
        personnel={personnelQuery.data?.items ?? []}
        shifts={shiftsQuery.data ?? []}
        isSubmitting={createAssignmentMutation.isPending}
        errorMessage={
          createAssignmentMutation.error ? getApiErrorMessage(createAssignmentMutation.error) : null
        }
        onClose={closeDialog}
        onSubmit={handleSubmit}
      />
    </Stack>
  )
}
