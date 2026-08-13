import { z } from 'zod'
import type { PersonnelFormValues } from './personnelTypes'

const optionalNumber = z
  .union([z.number(), z.nan(), z.null()])
  .transform((value) => (typeof value === 'number' && !Number.isNaN(value) ? value : null))

export const personnelFormSchema = z.object({
  registrationNumber: z.string().trim().min(1, 'Sicil numarası zorunludur.').max(50),
  firstName: z.string().trim().min(1, 'Ad zorunludur.').max(100),
  lastName: z.string().trim().min(1, 'Soyad zorunludur.').max(100),
  department: z.string().trim().max(150),
  title: z.string().trim().max(150),
  phone: z.string().trim().max(30),
  email: z
    .string()
    .trim()
    .max(200)
    .refine((value) => value === '' || z.email().safeParse(value).success, 'Geçerli e-posta girin.'),
  address: z.string().trim().max(500),
  latitude: optionalNumber.refine((value) => value === null || (value >= -90 && value <= 90), {
    message: 'Latitude -90 ile 90 arasında olmalıdır.',
  }),
  longitude: optionalNumber.refine((value) => value === null || (value >= -180 && value <= 180), {
    message: 'Longitude -180 ile 180 arasında olmalıdır.',
  }),
})

export type PersonnelFormSchema = PersonnelFormValues
