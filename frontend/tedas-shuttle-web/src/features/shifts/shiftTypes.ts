export const shiftTypes = [
  { value: 1, label: 'Sabah' },
  { value: 2, label: 'Aksam' },
  { value: 3, label: 'Ozel' },
] as const

export type ShiftType = (typeof shiftTypes)[number]['value']

export type ShiftListItem = {
  id: string
  physicalShuttleId: string
  physicalShuttleCode: string
  name: string
  shiftType: ShiftType
  capacity: number
  occupancy: number
  availableSeats: number
  startTime: string
  endTime: string
  isActive: boolean
}

export type Shift = ShiftListItem & {
  createdAt: string
  updatedAt: string | null
}

export type ShiftFormValues = {
  name: string
  shiftType: ShiftType
  capacity: number
  startTime: string
  endTime: string
}

