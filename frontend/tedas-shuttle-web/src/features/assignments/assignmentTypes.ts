export type AssignmentListItem = {
  id: string
  personnelId: string
  registrationNumber: string
  personnelFullName: string
  department: string | null
  shuttleShiftId: string
  physicalShuttleCode: string
  shiftName: string
  capacity: number
  occupancy: number
  availableSeats: number
  boardingRoutePointId: string | null
  isActive: boolean
  assignedAt: string
}

export type Assignment = AssignmentListItem & {
  deactivatedAt: string | null
}

export type AssignmentQuery = {
  page: number
  pageSize: number
  search?: string
  isActive?: boolean
}

export type AssignmentFormValues = {
  personnelId: string
  shuttleShiftId: string
}

