import axios from 'axios'

type ProblemDetails = {
  title?: string
  detail?: string
  code?: string
}

export function getApiErrorMessage(error: unknown) {
  if (axios.isAxiosError<ProblemDetails>(error)) {
    return error.response?.data.detail ?? error.response?.data.title ?? error.message
  }

  if (error instanceof Error) {
    return error.message
  }

  return 'Beklenmeyen bir hata oluştu.'
}
