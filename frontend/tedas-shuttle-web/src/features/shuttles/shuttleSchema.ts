import { z } from 'zod'
import type { ShuttleFormValues } from './shuttleTypes'

export const shuttleFormSchema = z.object({
  code: z.string().trim().min(1, 'Servis kodu zorunludur.').max(50),
  plateNumber: z.string().trim().min(1, 'Plaka zorunludur.').max(20),
  description: z.string().trim().max(500),
})

export type ShuttleFormSchema = ShuttleFormValues
