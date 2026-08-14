export type DashboardMetrics = {
  totalPersonnel: number
  activePersonnel: number
  totalShuttles: number
  activeShuttles: number
  totalShifts: number
  activeShifts: number
  assignedPersonnel: number
  unassignedPersonnel: number
  routePointCount: number
  savedRouteCount: number
}

export type ShiftOccupancy = {
  shuttleShiftId: string
  physicalShuttleCode: string
  shiftName: string
  capacity: number
  occupancy: number
  availableSeats: number
  utilizationPercent: number
  isActive: boolean
}

export type DashboardSummary = {
  metrics: DashboardMetrics
  shiftOccupancies: ShiftOccupancy[]
}
