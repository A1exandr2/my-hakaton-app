import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';

export default function AvailabilityChart({ servers }) {
  const chartData = servers.map(server => ({
    name: server.host,
    availability: server.status === 'works' ? 100 : 0,
  }));

  // Кастомный тултип
  const CustomTooltip = ({ active, payload, label }) => {
    if (active && payload && payload.length) {
      const isUp = payload[0].value === 100;
      return (
        <div className="bg-white p-3 rounded-lg border border-gray-200 shadow-lg">
          <p className="text-gray-800 font-semibold text-sm">{label}</p>
          <p className={`text-sm font-medium ${isUp ? 'text-green-600' : 'text-red-600'}`}>
            Статус: <span className="font-bold">{isUp ? 'Online' : 'Offline'}</span>
          </p>
          <p className="text-gray-600 text-sm">
            Доступность: <span className="font-bold">{payload[0].value}%</span>
          </p>
        </div>
      );
    }
    return null;
  };

  return (
    <div className="bg-white rounded-xl p-6 border border-gray-200 shadow-sm">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-lg font-semibold text-gray-800">Доступность серверов</h3>
        <div className="flex items-center space-x-4">
          <div className="flex items-center">
            <div className="w-3 h-3 bg-green-500 rounded-full mr-2"></div>
            <span className="text-sm text-gray-600">Online</span>
          </div>
          <div className="flex items-center">
            <div className="w-3 h-3 bg-red-500 rounded-full mr-2"></div>
            <span className="text-sm text-gray-600">Offline</span>
          </div>
        </div>
      </div>
      
      <div className="h-64">
        <ResponsiveContainer width="100%" height="100%">
          <LineChart
            data={chartData}
            margin={{ top: 5, right: 20, left: 20, bottom: 50 }}
          >
            <CartesianGrid 
              strokeDasharray="3 3" 
              stroke="#E5E7EB" 
            />
            <XAxis 
              dataKey="name" 
              stroke="#6B7280"
              fontSize={12}
              angle={-45}
              textAnchor="end"
              height={60}
              tick={{ fill: '#6B7280' }}
            />
            <YAxis 
              stroke="#6B7280"
              fontSize={12}
              domain={[0, 100]}
              tick={{ fill: '#6B7280' }}
              tickFormatter={(value) => `${value}%`}
              width={40}
            />
            <Tooltip content={<CustomTooltip />} />
            <Line 
              type="monotone" 
              dataKey="availability" 
              stroke="#10B981"
              strokeWidth={3}
              dot={(props) => {
                const { cx, cy, payload } = props;
                const isUp = payload.availability === 100;
                return (
                  <circle 
                    cx={cx} 
                    cy={cy} 
                    r={5} 
                    fill={isUp ? "#10B981" : "#EF4444"} 
                    stroke={isUp ? "#059669" : "#DC2626"} 
                    strokeWidth={2}
                  />
                );
              }}
              activeDot={{ r: 8, fill: "#10B981", stroke: "#059669", strokeWidth: 2 }}
            />
          </LineChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}