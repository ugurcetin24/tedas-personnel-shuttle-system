import { z } from 'zod'
import type { RoutePointFormValues } from './routePointTypes'

export const routePointFormSchema = z.object({
  name: z.string().trim().min(1, 'Nokta adi zorunludur.').max(150),
  address: z.string().trim().max(500),
  latitude: z.number().min(-90).max(90),
  longitude: z.number().min(-180).max(180),
})

export type RoutePointFormSchema = RoutePointFormValues

