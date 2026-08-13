export type PersonnelListItem = {
  id: string
  registrationNumber: string
  fullName: string
  department: string | null
  title: string | null
  phone: string | null
  email: string | null
  isActive: boolean
}

export type Personnel = PersonnelListItem & {
  firstName: string
  lastName: string
  address: string | null
  latitude: number | null
  longitude: number | null
  createdAt: string
  updatedAt: string | null
  currentAssignment: {
    assignmentId: string
    shuttleCode: string
    shiftName: string
    boardingPointName: string | null
  } | null
}

export type PersonnelQuery = {
  page: number
  pageSize: number
  search?: string
  department?: string
  isActive?: boolean
}

export type PersonnelFormValues = {
  registrationNumber: string
  firstName: string
  lastName: string
  department: string
  title: string
  phone: string
  email: string
  address: string
  latitude: number | null
  longitude: number | null
}

export type UpdatePersonnelPayload = Omit<PersonnelFormValues, 'registrationNumber'>
