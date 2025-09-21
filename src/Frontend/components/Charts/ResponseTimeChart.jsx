import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Cell } from 'recharts';

export default function HorizontalResponseTimeChart({ servers }) {
  const chartData = servers.map(server => ({
    name: server.host,
    responseTime: server.stats?.avgResponseTimeMs || 0,
    status: server.status
  }));

  // Сортируем по времени отклика
  chartData.sort((a, b) => b.responseTime - a.responseTime);

  const getBarColor = (status, responseTime) => {
    if (status !== 'works') return '#EF4444';
    if (responseTime > 1000) return '#F59E0B';
    if (responseTime > 500) return '#F97316';
    return '#10B981';
  };

  // Кастомный тултип
  const CustomTooltip = ({ active, payload, label }) => {
    if (active && payload && payload.length) {
      const data = payload[0].payload;
      const isUp = data.status === 'works';
      
      return (
        <div className="bg-white p-3 rounded-lg border border-gray-200 shadow-lg">
          <p className="text-gray-800 font-semibold text-sm">{label}</p>
          <p className={`text-sm font-medium ${isUp ? 'text-green-600' : 'text-red-600'}`}>
            Статус: <span className="font-bold">{isUp ? 'Online' : 'Offline'}</span>
          </p>
          <p className="text-gray-600 text-sm">
            Время ответа: <span className="font-bold">{payload[0].value} мс</span>
          </p>
          {isUp && (
            <p className="text-xs text-gray-500 mt-1">
              {payload[0].value > 1000 ? 'Медленно' : payload[0].value > 500 ? 'Средне' : 'Быстро'}
            </p>
          )}
        </div>
      );
    }
    return null;
  };

  return (
    <div className="bg-white rounded-xl p-6 border border-gray-200 shadow-sm">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-lg font-semibold text-gray-800">Время ответа серверов</h3>
        <div className="flex items-center space-x-4">
          <div className="flex items-center">
            <div className="w-3 h-3 bg-green-500 rounded-full mr-2"></div>
            <span className="text-sm text-gray-600">{"< 500мс"}</span>
          </div>
          <div className="flex items-center">
            <div className="w-3 h-3 bg-amber-500 rounded-full mr-2"></div>
            <span className="text-sm text-gray-600">500-1000мс</span>
          </div>
          <div className="flex items-center">
            <div className="w-3 h-3 bg-orange-500 rounded-full mr-2"></div>
            <span className="text-sm text-gray-600">{"> 1000мс"}</span>
          </div>
          <div className="flex items-center">
            <div className="w-3 h-3 bg-red-500 rounded-full mr-2"></div>
            <span className="text-sm text-gray-600">Offline</span>
          </div>
        </div>
      </div>

      <div className="h-80">
        <ResponsiveContainer width="100%" height="100%">
          <BarChart 
            data={chartData} 
            layout="vertical"
            margin={{ top: 5, right: 20, left: 100, bottom: 20 }}
          >
            <CartesianGrid 
              strokeDasharray="3 3" 
              stroke="#E5E7EB" 
              horizontal={false}
            />
            <XAxis 
              type="number" 
              stroke="#6B7280"
              fontSize={12}
              tick={{ fill: '#6B7280' }}
              label={{ 
                value: 'миллисекунды', 
                position: 'insideBottom', 
                offset: -5,
                style: { fill: '#6B7280', fontSize: 12 } 
              }}
            />
            <YAxis 
              type="category" 
              dataKey="name" 
              stroke="#6B7280"
              fontSize={12}
              tick={{ fill: '#6B7280' }}
              width={90}
            />
            <Tooltip content={<CustomTooltip />} />
            <Bar 
              dataKey="responseTime" 
              radius={[0, 4, 4, 0]}
              maxBarSize={40}
            >
              {chartData.map((entry, index) => (
                <Cell 
                  key={`cell-${index}`} 
                  fill={getBarColor(entry.status, entry.responseTime)}
                />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}