export type RouteCoordinate = {
  latitude: number
  longitude: number
}

export type CalculatedRoute = {
  distanceMeters: number
  durationSeconds: number
  geometry: string
  coordinates: RouteCoordinate[]
}

export type SavedRoute = {
  id: string
  shuttleShiftId: string
  physicalShuttleCode: string
  shiftName: string
  name: string
  distanceMeters: number
  durationSeconds: number
  geometry: string
  createdAt: string
  updatedAt: string | null
}
