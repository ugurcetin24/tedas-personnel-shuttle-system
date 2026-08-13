import { z } from 'zod'
import type { DriverFormValues } from './driverTypes'

export const driverFormSchema = z.object({
  firstName: z.string().trim().min(1, 'Ad zorunludur.').max(100),
  lastName: z.string().trim().min(1, 'Soyad zorunludur.').max(100),
  phone: z.string().trim().max(30),
  licenseNumber: z.string().trim().min(1, 'Ehliyet numarasi zorunludur.').max(50),
})

export type DriverFormSchema = DriverFormValues

