import { useState } from 'react';
import { useRouter } from 'next/router';

export default function ServerForm({ initialData = {}, onSubmit, loading = false }) {
  const [formData, setFormData] = useState({
    host: initialData.host || '',
    ip: initialData.ip || '',
    interval: initialData.interval || '5m',
    routes: initialData.routes || [{ route: '/' }]
  });
  
  const router = useRouter();

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      await onSubmit(formData);
    } catch (error) {
      console.error('Error submitting form:', error);
    }
  };

  const addRoute = () => {
    setFormData(prev => ({
      ...prev,
      routes: [...prev.routes, { route: '/' }]
    }));
  };

  const removeRoute = (index) => {
    setFormData(prev => ({
      ...prev,
      routes: prev.routes.filter((_, i) => i !== index)
    }));
  };

  const updateRoute = (index, value) => {
    setFormData(prev => ({
      ...prev,
      routes: prev.routes.map((route, i) => 
        i === index ? { ...route, route: value } : route
      )
    }));
  };

  return (
    <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
      <h2 className="text-xl font-semibold text-gray-900 mb-6">
        {initialData.id ? 'Редактирование сервера' : 'Добавление нового сервера'}
      </h2>

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Хост */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Хост *
          </label>
          <input
            type="text"
            value={formData.host}
            onChange={(e) => setFormData({ ...formData, host: e.target.value })}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors text-gray-900 disabled:bg-gray-100 disabled:text-gray-500"
            placeholder="example.com"
            required
            disabled={loading}
          />
          <p className="text-xs text-gray-500 mt-1">
            Доменное имя сервера для мониторинга
          </p>
        </div>

        {/* IP адрес */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            IP адрес *
          </label>
          <input
            type="text"
            value={formData.ip}
            onChange={(e) => setFormData({ ...formData, ip: e.target.value })}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors text-gray-900 disabled:bg-gray-100 disabled:text-gray-500"
            placeholder="192.168.1.1"
            required
            disabled={loading}
          />
          <p className="text-xs text-gray-500 mt-1">
            IP адрес сервера для прямого доступа
          </p>
        </div>

        {/* Интервал проверки */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Интервал проверки *
          </label>
          <select
            value={formData.interval}
            onChange={(e) => setFormData({ ...formData, interval: e.target.value })}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors text-gray-900 disabled:bg-gray-100 disabled:text-gray-500"
            disabled={loading}
          >
            <option value="1m">1 минута</option>
            <option value="5m">5 минут</option>
            <option value="15m">15 минут</option>
            <option value="30m">30 минут</option>
            <option value="1h">1 час</option>
          </select>
          <p className="text-xs text-gray-500 mt-1">
            Как часто проверять доступность сервера
          </p>
        </div>
        

        {/* Кнопки действий */}
        <div className="flex space-x-4 pt-4 border-t border-gray-200">
          <button
            type="submit"
            disabled={loading}
            className="inline-flex items-center px-4 py-2 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {loading ? (
              <>
                <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                Сохранение...
              </>
            ) : (
              <>
                <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7m-8 5v6m-6-6h12" />
                </svg>
                {initialData.id ? 'Обновить сервер' : 'Добавить сервер'}
              </>
            )}
          </button>
          
          <button
            type="button"
            onClick={() => router.back()}
            disabled={loading}
            className="inline-flex items-center px-4 py-2 bg-gray-100 text-gray-700 rounded-lg font-medium hover:bg-gray-200 transition-colors border border-gray-300 disabled:opacity-50"
          >
            <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
            Отмена
          </button>
        </div>
      </form>
    </div>
  );
}