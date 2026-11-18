import {
  Area,
  AreaChart,
  CartesianGrid,
  Legend,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import { useLanguage } from '@/core/context/useLanguage'
import type { StockMovementTrendChartProps } from '@/routes/dashboard/components/StockMovementTrendChart/type'

const tooltipStyle = {
  backgroundColor: 'rgb(var(--bg-surface))',
  borderColor: 'rgb(var(--border-subtle))',
  borderRadius: '0.75rem',
  color: 'rgb(var(--text-primary))',
}

export function StockMovementTrendChart({ data, isLoading }: StockMovementTrendChartProps) {
  const { t } = useLanguage()

  if (isLoading) {
    return <div className="h-80 animate-pulse rounded-2xl border border-subtle bg-surface dark:border-white/10" />
  }

  if (!data.length) {
    return (
      <div className="flex h-80 items-center justify-center rounded-2xl border border-subtle bg-surface text-sm text-muted dark:border-white/10">
        {t('dashboard.empty')}
      </div>
    )
  }

  return (
    <div className="space-y-4 rounded-2xl border border-subtle bg-surface p-5 dark:border-white/10">
      <h2 className="text-base font-semibold text-primary">{t('dashboard.charts.stockMovementTitle')}</h2>
      <div className="h-72 w-full">
        <ResponsiveContainer width="100%" height="100%">
          <AreaChart data={data}>
            <defs>
              <linearGradient id="inboundGradient" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor="rgb(var(--brand-primary))" stopOpacity={0.35} />
                <stop offset="95%" stopColor="rgb(var(--brand-primary))" stopOpacity={0} />
              </linearGradient>
              <linearGradient id="outboundGradient" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor="rgb(var(--text-muted))" stopOpacity={0.25} />
                <stop offset="95%" stopColor="rgb(var(--text-muted))" stopOpacity={0} />
              </linearGradient>
            </defs>
            <CartesianGrid strokeDasharray="3 3" stroke="rgb(var(--border-subtle))" vertical={false} />
            <XAxis dataKey="date" stroke="rgb(var(--text-muted))" fontSize={12} tickMargin={8} />
            <YAxis stroke="rgb(var(--text-muted))" fontSize={12} tickMargin={8} />
            <Tooltip contentStyle={tooltipStyle} />
            <Legend />
            <Area
              type="monotone"
              dataKey="inbound"
              name={t('dashboard.charts.inbound')}
              stroke="rgb(var(--brand-primary))"
              strokeWidth={2}
              fill="url(#inboundGradient)"
            />
            <Area
              type="monotone"
              dataKey="outbound"
              name={t('dashboard.charts.outbound')}
              stroke="rgb(var(--text-muted))"
              strokeWidth={2}
              fill="url(#outboundGradient)"
            />
          </AreaChart>
        </ResponsiveContainer>
      </div>
    </div>
  )
}
