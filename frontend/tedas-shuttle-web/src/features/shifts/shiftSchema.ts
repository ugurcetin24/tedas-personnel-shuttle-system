import { z } from 'zod'
import type { ShiftFormValues } from './shiftTypes'

export const shiftFormSchema = z.object({
  name: z.string().trim().min(1, 'Vardiya adi zorunludur.').max(100),
  shiftType: z.union([z.literal(1), z.literal(2), z.literal(3)]),
  capacity: z.number().int().min(1, 'Kapasite en az 1 olmalidir.'),
  startTime: z.string().trim().min(1, 'Baslangic saati zorunludur.'),
  endTime: z.string().trim().min(1, 'Bitis saati zorunludur.'),
})

export type ShiftFormSchema = ShiftFormValues
