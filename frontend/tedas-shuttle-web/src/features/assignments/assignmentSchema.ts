import { z } from 'zod'
import type { AssignmentFormValues } from './assignmentTypes'

export const assignmentFormSchema = z.object({
  personnelId: z.string().trim().min(1, 'Personel secimi zorunludur.'),
  shuttleShiftId: z.string().trim().min(1, 'Vardiya secimi zorunludur.'),
})

export type AssignmentFormSchema = AssignmentFormValues

