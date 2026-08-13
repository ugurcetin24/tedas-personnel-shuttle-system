export type ShuttleListItem = {
  id: string
  code: string
  plateNumber: string
  description: string | null
  isActive: boolean
}

export type Shuttle = ShuttleListItem & {
  createdAt: string
  updatedAt: string | null
}

export type ShuttleQuery = {
  page: number
  pageSize: number
  code?: string
  plateNumber?: string
  isActive?: boolean
}

export type ShuttleFormValues = {
  code: string
  plateNumber: string
  description: string
}

export type UpdateShuttlePayload = Omit<ShuttleFormValues, 'code'>
