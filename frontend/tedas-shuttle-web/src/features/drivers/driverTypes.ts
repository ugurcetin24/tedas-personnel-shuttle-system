import type { ShiftType } from '../shifts/shiftTypes'

export type DriverShiftAssignment = {
  shuttleShiftId: string
  physicalShuttleId: string
  physicalShuttleCode: string
  shiftName: string
  shiftType: ShiftType
}

export type DriverListItem = {
  id: string
  firstName: string
  lastName: string
  fullName: string
  phone: string | null
  licenseNumber: string
  isActive: boolean
  assignedShift: DriverShiftAssignment | null
}

export type Driver = DriverListItem & {
  createdAt: string
  updatedAt: string | null
}

export type DriverQuery = {
  page: number
  pageSize: number
  search?: string
  isActive?: boolean
}

export type DriverFormValues = {
  firstName: string
  lastName: string
  phone: string
  licenseNumber: string
}

export type UpdateDriverPayload = DriverFormValues

