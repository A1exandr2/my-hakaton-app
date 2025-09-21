import { useState, useEffect } from 'react';
import { useRouter } from 'next/router';
import Head from 'next/head';
import Link from 'next/link';
import { api } from '../../utils/api';
import ServerTable from '../../components/ServerTable';

export default function ServersPage() {
  const [servers, setServers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [refreshing, setRefreshing] = useState(false);
  const router = useRouter();

  const limit = 25;

  useEffect(() => {
    loadServers();
  }, [currentPage, searchTerm]);

  const loadServers = async () => {
    try {
      setLoading(true);
      const params = {
        limit,
        offset: (currentPage - 1) * limit,
        ...(searchTerm && { query: searchTerm })
      };
      
      const response = await api.getServers(params);
      
      const normalizedServers = (response.servers || response).map(server => ({
        ...server,
        status: server.status === 'works' ? 'works' : "doesn't work"
      }));
      
      setServers(normalizedServers);
      setTotalCount(response.totalCount || response.length);
      
    } catch (error) {
      console.error('Error loading servers:', error);
      alert('Ошибка загрузки серверов');
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  const handleDelete = async (id) => {
    if (confirm('Вы уверены, что хотите удалить этот сервер?')) {
      try {
        await api.deleteServer(id);
        loadServers();
      } catch (error) {
        console.error('Error deleting server:', error);
        alert('Ошибка удаления сервера');
      }
    }
  };

  const handlePageChange = (page) => {
    setCurrentPage(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const handleSearchChange = (value) => {
    setSearchTerm(value);
    setCurrentPage(1);
  };

  const handleRefresh = () => {
    setRefreshing(true);
    loadServers();
  };

  const totalPages = Math.ceil(totalCount / limit);

  const getPaginationRange = () => {
    const delta = 2;
    const range = [];
    const rangeWithDots = [];

    for (let i = 1; i <= totalPages; i++) {
      if (
        i === 1 ||
        i === totalPages ||
        (i >= currentPage - delta && i <= currentPage + delta)
      ) {
        range.push(i);
      }
    }

    let prev = 0;
    for (const i of range) {
      if (i - prev > 1) {
        rangeWithDots.push('...');
      }
      rangeWithDots.push(i);
      prev = i;
    }

    return rangeWithDots;
  };

  return (
    <>
      <Head>
        <title>Управление серверами - Мониторинг серверов</title>
      </Head>

      <div className="min-h-screen bg-gray-50 p-6">
        <div className="max-w-7xl mx-auto">
          {/* Заголовок и навигация */}
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
            <div className="flex items-center space-x-4">
              <Link 
                href="/"
                className="inline-flex items-center px-3 py-2 bg-white text-gray-700 rounded-lg border border-gray-300 hover:bg-gray-50 transition-colors"
              >
                <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
                </svg>
                К дашборду
              </Link>
              <div>
                <h1 className="text-2xl font-bold text-gray-900">Управление серверами</h1>
                <p className="text-gray-600 text-sm mt-1">
                  Всего серверов: {totalCount}
                </p>
              </div>
            </div>
          </div>

          {/* Поиск и действия */}
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6 mb-6">
            <div className="flex flex-col md:flex-row gap-4 justify-between items-start md:items-center">
              <div className="flex-1 w-full">
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <svg className="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                    </svg>
                  </div>
                  <input
                    type="text"
                    placeholder="Поиск серверов по имени или IP..."
                    value={searchTerm}
                    onChange={(e) => handleSearchChange(e.target.value)}
                    className="pl-10 pr-4 py-2 w-full border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors text-gray-900"
                  />
                </div>
              </div>
              
              <div className="flex flex-col sm:flex-row gap-3 w-full md:w-auto">
                <button
                  onClick={handleRefresh}
                  disabled={refreshing}
                  className="inline-flex items-center px-4 py-2 bg-white text-gray-700 rounded-lg border border-gray-300 hover:bg-gray-50 transition-colors disabled:opacity-50"
                >
                  <svg 
                    className={`w-4 h-4 mr-2 ${refreshing ? 'animate-spin' : ''}`} 
                    fill="none" 
                    stroke="currentColor" 
                    viewBox="0 0 24 24"
                  >
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                  </svg>
                  {refreshing ? 'Обновление...' : 'Обновить'}
                </button>
                
                <Link
                  href="/servers/add"
                  className="inline-flex items-center px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors text-center justify-center"
                >
                  <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                  </svg>
                  Добавить сервер
                </Link>
              </div>
            </div>
          </div>

          {/* Таблица серверов */}
          {loading ? (
            <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-8 text-center">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
              <div className="text-gray-600">Загрузка серверов...</div>
            </div>
          ) : (
            <>
              <ServerTable 
                servers={servers} 
                onDelete={handleDelete}
                onRefresh={loadServers}
              />
              
              {/* Пагинация */}
              {totalPages > 1 && (
                <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6 mt-6">
                  <div className="flex flex-col sm:flex-row items-center justify-between gap-4">
                    <div className="text-sm text-gray-600">
                      Показано {servers.length} из {totalCount} серверов
                    </div>
                    
                    <div className="flex items-center space-x-1">
                      <button
                        onClick={() => handlePageChange(currentPage - 1)}
                        disabled={currentPage === 1}
                        className="p-2 rounded-lg border border-gray-300 text-gray-600 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                      >
                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                        </svg>
                      </button>
                      
                      {getPaginationRange().map((page, index) => (
                        page === '...' ? (
                          <span key={index} className="px-2 py-1 text-gray-400">
                            ...
                          </span>
                        ) : (
                          <button
                            key={index}
                            onClick={() => handlePageChange(page)}
                            className={`px-3 py-1 rounded-lg text-sm font-medium transition-colors ${
                              currentPage === page
                                ? 'bg-blue-600 text-white'
                                : 'text-gray-600 hover:bg-gray-100'
                            }`}
                          >
                            {page}
                          </button>
                        )
                      ))}
                      
                      <button
                        onClick={() => handlePageChange(currentPage + 1)}
                        disabled={currentPage === totalPages}
                        className="p-2 rounded-lg border border-gray-300 text-gray-600 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                      >
                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                        </svg>
                      </button>
                    </div>
                    
                    <div className="text-sm text-gray-600">
                      Страница {currentPage} из {totalPages}
                    </div>
                  </div>
                </div>
              )}

              {servers.length === 0 && searchTerm && (
                <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-8 text-center">
                  <div className="text-gray-400 text-lg mb-2">Серверы не найдены</div>
                  <div className="text-gray-500 text-sm">
                    Попробуйте изменить поисковый запрос или{' '}
                    <Link href="/servers/new" className="text-blue-600 hover:text-blue-800">
                      добавьте новый сервер
                    </Link>
                  </div>
                </div>
              )}

              {servers.length === 0 && !searchTerm && (
                <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-8 text-center">
                  <div className="text-gray-400 text-lg mb-4">Серверы не найдены</div>
                  <Link
                    href="/servers/new"
                    className="inline-flex items-center px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
                  >
                    <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                    </svg>
                    Добавить первый сервер
                  </Link>
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </>
  );
}