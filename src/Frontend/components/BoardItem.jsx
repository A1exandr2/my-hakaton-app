import { useRouter } from "next/router";
import { useState } from "react";

export function BoardItem({ server }) {
  const [showLogs, setShowLogs] = useState(false);
  const router = useRouter();

  return (
    <tr
      className="border-b border-gray-700 hover:bg-gray-800 transition-colors"
      onClick={() => router.push(`/${server.name}`)}
    >
      <td className="px-4 py-3 text-white">{server.name}</td>
      <td className="px-4 py-3 text-white">{server.ip}</td>
      <td className="px-4 py-3">
        <span
          className={`px-2 py-1 rounded-full text-xs font-medium ${
            server.status === "works"
              ? "bg-green-500/20 text-green-400"
              : "bg-red-500/20 text-red-400"
          }`}
        >
          {server.status}
        </span>
      </td>
      <td className="px-4 py-3">
        <button
          onClick={(e) => {
            e.stopPropagation();
            setShowLogs((prev) => !prev);
          }}
          className="text-blue-400 hover:text-blue-300 text-sm"
        >
          {showLogs ? "Скрыть" : "Показать"}
        </button>
      </td>
      <td className="px-4 py-3">
        <div className="flex items-center space-x-1">
          <span className="text-yellow-400 text-sm">⚡</span>
          <span className="text-white text-sm">
            {server.stats.avgResponseTimeMs} ms
          </span>
        </div>
      </td>

      {showLogs && (
        <td className="px-4 py-2 bg-gray-900/50 max-w-[120px] min-w-[120px]">
          <div className="space-y-1 max-h-20 overflow-y-auto">
            {server.logs.map((log, i) => (
              <div key={i} className="text-xs text-gray-300">
                <span>{log.timestamp}</span> •{" "}
                <span
                  className={log.success ? "text-green-400" : "text-red-400"}
                >
                  {log.success ? "OK" : "ERR"} ({log.responseTimeMs}ms)
                </span>
              </div>
            ))}
          </div>
        </td>
      )}
    </tr>
  );
}
