import type { AxiosRequestConfig } from 'axios'
import { axiosClient } from '../core/api/axiosClient'
import type { PagedResult } from '../types/api'
import type {
  InboundMovementRequest,
  OutboundMovementRequest,
  StockBalance,
  StockBalanceQuery,
  StockMovement,
  StockMovementQuery,
  TransferMovementRequest,
} from '../types/stock'

const MOVEMENTS = '/stock-movements'
const BALANCES = '/stock-balances'

/** Hareket kaydedenler hareketin kimliğini döndürür; bakiyeler yalnızca hareketlerle değişir. */
export const stockService = {
  async listMovements(query: StockMovementQuery, signal?: AbortSignal): Promise<PagedResult<StockMovement>> {
    const { data } = await axiosClient.get<PagedResult<StockMovement>>(MOVEMENTS, { params: query, signal })
    return data
  },

  async inbound(request: InboundMovementRequest, config?: AxiosRequestConfig): Promise<string> {
    const { data } = await axiosClient.post<string>(`${MOVEMENTS}/inbound`, request, config)
    return data
  },

  async outbound(request: OutboundMovementRequest, config?: AxiosRequestConfig): Promise<string> {
    const { data } = await axiosClient.post<string>(`${MOVEMENTS}/outbound`, request, config)
    return data
  },

  async transfer(request: TransferMovementRequest, config?: AxiosRequestConfig): Promise<string> {
    const { data } = await axiosClient.post<string>(`${MOVEMENTS}/transfer`, request, config)
    return data
  },

  async listBalances(query: StockBalanceQuery, signal?: AbortSignal): Promise<PagedResult<StockBalance>> {
    const { data } = await axiosClient.get<PagedResult<StockBalance>>(BALANCES, { params: query, signal })
    return data
  },
}
