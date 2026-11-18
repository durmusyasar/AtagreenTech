import { Cell, Pie, PieChart, ResponsiveContainer, Tooltip } from 'recharts'
import { useLanguage } from '@/core/context/useLanguage'
import type { WarehouseDistributionChartProps } from '@/routes/dashboard/components/WarehouseDistributionChart/type'

const COLORS = [
  'rgb(var(--brand-primary))',
  'rgb(var(--text-muted))',
  'rgb(34 197 94)',
  'rgb(59 130 246)',
  'rgb(245 158 11)',
  'rgb(168 85 247)',
  'rgb(14 165 233)',
  'rgb(244 63 94)',
]

const tooltipStyle = {
  backgroundColor: 'rgb(var(--bg-surface))',
  borderColor: 'rgb(var(--border-subtle))',
  borderRadius: '0.75rem',
  color: 'rgb(var(--text-primary))',
}

export function WarehouseDistributionChart({ data, isLoading }: WarehouseDistributionChartProps) {
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
      <h2 className="text-base font-semibold text-primary">{t('dashboard.charts.warehouseTitle')}</h2>
      <div className="h-72 w-full">
        <ResponsiveContainer width="100%" height="100%">
          <PieChart>
            <Pie data={data} dataKey="value" nameKey="name" innerRadius={55} outerRadius={95} paddingAngle={2}>
              {data.map((entry, index) => (
                <Cell key={entry.name} fill={COLORS[index % COLORS.length]} />
              ))}
            </Pie>
            <Tooltip contentStyle={tooltipStyle} />
          </PieChart>
        </ResponsiveContainer>
      </div>
      <ul className="grid gap-1.5 sm:grid-cols-2">
        {data.map((item, index) => (
          <li key={item.name} className="flex items-center gap-2 text-xs text-secondary">
            <span
              className="h-2.5 w-2.5 rounded-full"
              style={{ backgroundColor: COLORS[index % COLORS.length] }}
              aria-hidden
            />
            <span className="truncate">{item.name}</span>
            <span className="ml-auto font-medium text-primary">{item.value.toLocaleString()}</span>
          </li>
        ))}
      </ul>
    </div>
  )
}
