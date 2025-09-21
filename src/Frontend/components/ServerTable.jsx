import Link from 'next/link';

export default function ServerTable({ servers, onDelete, onRefresh, showActions = true }) {
  const getStatusConfig = (status) => {
    const config = {
      up: {
        color: 'text-green-600',
        bg: 'bg-green-100',
        border: 'border-green-200',
        icon: (
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
          </svg>
        )
      },
      down: {
        color: 'text-red-600',
        bg: 'bg-red-100',
        border: 'border-red-200',
        icon: (
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          </svg>
        )
      }
    };
    return config[status] || config.down;
  };

  const getSuccessRateColor = (rate) => {
    if (rate >= 95) return 'text-green-600';
    if (rate >= 80) return 'text-amber-600';
    return 'text-red-600';
  };

  const getResponseTimeColor = (time) => {
    if (time <= 200) return 'text-green-600';
    if (time <= 500) return 'text-amber-600';
    return 'text-red-600';
  };

  return (
    <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div className="overflow-x-auto">
        <table className="w-full">
          <thead>
            <tr className="bg-gray-50 border-b border-gray-200">
              <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Хост
              </th>
              <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Статус
              </th>
              <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Протокол
              </th>
              <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Успешность
              </th>
              <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Сообщение
              </th>
              <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Статус код
                </th>
              {showActions && (
                <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Действия
                </th>
              )}
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {servers.map((server) => {
              const statusConfig = getStatusConfig(server.status);
              return (
                <tr 
                  key={server.id} 
                  className="hover:bg-gray-50 transition-colors duration-150"
                >
                  {/* Хост */}
                  <td className="px-6 py-4">
                    <Link 
                      href={`/servers/${server.id}`}
                      className="text-blue-600 hover:text-blue-800 font-medium transition-colors"
                    >
                      {server.host}
                    </Link>
                    {server.ip && (
                      <div className="text-sm text-gray-500 font-mono mt-1">
                        {server.ip}
                      </div>
                    )}
                  </td>

                  {/* Статус */}
                  <td className="px-6 py-4">
                    <div className={`inline-flex items-center px-3 py-1 rounded-full text-sm font-medium border ${statusConfig.bg} ${statusConfig.color} ${statusConfig.border}`}>
                      <span className="mr-2">{statusConfig.icon}</span>
                      {server.status === 'works' ? 'Online' : 'Offline'}
                    </div>
                  </td>

                  {/* Протокол */}
                  <td className="px-6 py-4">
                    <div className="flex items-center">
                      <span className={`font-medium ${getResponseTimeColor(server.protocol)}`}>
                        {server.protocol}
                      </span>
                    </div>
                  </td>

                  {/* Успешность */}
                  <td className="px-6 py-4">
                    <div className="flex items-center">
                      <span className={`font-medium ${getSuccessRateColor(server.stats?.successRate)}`}>
                        {server.stats?.successRate || 0}%
                      </span>
                    </div>
                  </td>

                  {/* errorMessage */}
                  <td className="px-6 py-4">
                    <div className="flex items-center">
                      <span>{server.errorMessage}</span>
                    </div>
                  </td>
                  
                  <td className="px-6 py-4">
                    <div className="flex items-center">
                      <span className={`font-medium ${getSuccessRateColor(server.stats?.successRate)}`}>{server.statusCode}</span>
                    </div>
                  </td>

                  {/* Действия */}
                  {showActions && (
                    <td className="px-6 py-4">
                      <div className="flex space-x-2">
                        <Link
                          href={`/servers/${server.id}/edit`}
                          className="inline-flex items-center px-3 py-2 bg-blue-50 text-blue-600 rounded-lg text-sm font-medium hover:bg-blue-100 transition-colors border border-blue-200"
                        >
                          <svg className="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                          </svg>
                          Изменить
                        </Link>
                        <button
                          onClick={() => onDelete(server.id)}
                          className="inline-flex items-center px-3 py-2 bg-red-50 text-red-600 rounded-lg text-sm font-medium hover:bg-red-100 transition-colors border border-red-200"
                        >
                          <svg className="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                          </svg>
                          Удалить
                        </button>
                      </div>
                    </td>
                  )}
                </tr>
              );
            })}
          </tbody>
        </table>

        {servers.length === 0 && (
          <div className="text-center py-12">
            <div className="text-gray-400 text-lg">Нет серверов для отображения</div>
            <div className="text-gray-500 text-sm mt-2">Добавьте первый сервер для мониторинга</div>
          </div>
        )}
      </div>
    </div>
  );
}