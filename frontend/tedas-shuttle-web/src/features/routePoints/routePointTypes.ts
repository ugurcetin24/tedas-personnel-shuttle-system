export type RoutePointListItem = {
  id: string
  shuttleShiftId: string
  physicalShuttleCode: string
  shiftName: string
  order: number
  name: string
  address: string | null
  latitude: number
  longitude: number
  isActive: boolean
}

export type RoutePoint = RoutePointListItem & {
  createdAt: string
  updatedAt: string | null
}

export type RoutePointFormValues = {
  name: string
  address: string
  latitude: number
  longitude: number
}

