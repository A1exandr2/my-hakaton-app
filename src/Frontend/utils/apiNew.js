import { logs, servers, serverStats, stats } from "./placeholder";

const API_BASE = process.env.NEXT_PUBLIC_API_BASE_URL || 'backend:8080';

export const api = {

  // Получение списка серверов
  getServers: async (params = {}) => {
    const queryParams = new URLSearchParams(params).toString();
    const response = await fetch(`${API_BASE}/api/servers?${queryParams}`);
    return response.json();

  },

  // Добавление сервера
  addServer: async (serverData) => {
    const response = await fetch(`${API_BASE}/api/servers`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(serverData),
    });
    return response.json();
  },

  // Получение данных сервера - ИСПРАВЛЕНО
  getServer: async (id) => {
    const response = await fetch(`${API_BASE}/api/servers/${id}`);
    return response.json();
    
  },

  // Обновление сервера
  updateServer: async (id, updateData) => {
    const response = await fetch(`${API_BASE}/api/servers/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(updateData),
    });
    return response.json();
  },

  // Удаление сервера
  deleteServer: async (id) => {
    const response = await fetch(`${API_BASE}/api/servers/${id}`, {
      method: 'DELETE',
    });
    return response.json();
  },

  // Получение логов сервера
  getServerLogs: async (id, params = {}) => {
    const queryParams = new URLSearchParams(params).toString();
    const response = await fetch(`${API_BASE}/api/servers/${id}/logs?${queryParams}`);
    return response.json();

  },

  // Получение статистики сервера - ИСПРАВЛЕНО
  getServerStats: async (id, period = '24h') => {
    const response = await fetch(`${API_BASE}/api/servers/${id}/stats?period=${period}`);
    return response.json();

  },

  // Получение общей статистики
  getOverviewStats: async () => {
    const response = await fetch(`${API_BASE}/api/stats/overview`);
    return response.json();

  },
};