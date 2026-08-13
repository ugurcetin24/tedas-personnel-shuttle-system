import { zodResolver } from '@hookform/resolvers/zod'
import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  Grid,
  InputLabel,
  MenuItem,
  Select,
} from '@mui/material'
import { useEffect } from 'react'
import { Controller, useForm } from 'react-hook-form'
import type { PersonnelListItem } from '../personnel/personnelTypes'
import { shiftTypes, type ShiftListItem } from '../shifts/shiftTypes'
import { assignmentFormSchema, type AssignmentFormSchema } from './assignmentSchema'
import type { AssignmentFormValues } from './assignmentTypes'

type AssignmentFormDialogProps = {
  open: boolean
  personnel: PersonnelListItem[]
  shifts: ShiftListItem[]
  isSubmitting: boolean
  errorMessage?: string | null
  onClose: () => void
  onSubmit: (values: AssignmentFormValues) => void
}

const defaultValues: AssignmentFormValues = {
  personnelId: '',
  shuttleShiftId: '',
}

export function AssignmentFormDialog({
  open,
  personnel,
  shifts,
  isSubmitting,
  errorMessage,
  onClose,
  onSubmit,
}: AssignmentFormDialogProps) {
  const form = useForm<AssignmentFormSchema>({
    resolver: zodResolver(assignmentFormSchema),
    defaultValues,
  })

  useEffect(() => {
    if (open) {
      form.reset(defaultValues)
    }
  }, [form, open])

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>Servis Atamasi Baslat</DialogTitle>
      <DialogContent>
        {errorMessage ? (
          <Alert severity="error" sx={{ mb: 2 }}>
            {errorMessage}
          </Alert>
        ) : null}
        <Grid
          container
          spacing={2}
          component="form"
          id="assignment-form"
          onSubmit={form.handleSubmit((values) => onSubmit(values))}
          sx={{ pt: 1 }}
        >
          <Grid size={{ xs: 12, md: 6 }}>
            <Controller
              name="personnelId"
              control={form.control}
              render={({ field, fieldState }) => (
                <FormControl size="small" fullWidth error={!!fieldState.error}>
                  <InputLabel id="assignment-personnel-label">Personel</InputLabel>
                  <Select {...field} labelId="assignment-personnel-label" label="Personel">
                    {personnel.map((item) => (
                      <MenuItem key={item.id} value={item.id}>
                        {item.registrationNumber} - {item.fullName}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              )}
            />
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <Controller
              name="shuttleShiftId"
              control={form.control}
              render={({ field, fieldState }) => (
                <FormControl size="small" fullWidth error={!!fieldState.error}>
                  <InputLabel id="assignment-shift-label">Vardiya</InputLabel>
                  <Select {...field} labelId="assignment-shift-label" label="Vardiya">
                    {shifts.map((shift) => (
                      <MenuItem key={shift.id} value={shift.id}>
                        {shift.physicalShuttleCode} / {shift.name} ({getShiftTypeLabel(shift.shiftType)}) -{' '}
                        {shift.availableSeats} bos
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              )}
            />
          </Grid>
        </Grid>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Vazgec</Button>
        <Button type="submit" form="assignment-form" variant="contained" disabled={isSubmitting}>
          Kaydet
        </Button>
      </DialogActions>
    </Dialog>
  )
}

function getShiftTypeLabel(value: number) {
  return shiftTypes.find((type) => type.value === value)?.label ?? '-'
}

