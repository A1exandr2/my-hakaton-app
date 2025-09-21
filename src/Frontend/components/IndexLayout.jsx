
import { BoardItem } from "./BoardItem";

export function IndexLayout() {
  return (
    <div className="p-4">
      <table className="w-full bg-black/40 text-white border-collapse">
        <thead>
          <tr className="bg-gray-800/50">
            <th className="px-4 py-3 text-left">ID</th>
            <th className="px-4 py-3 text-left">IP</th>
            <th className="px-4 py-3 text-left">Status</th>
            <th className="px-4 py-3 text-left">Logs</th>
            <th className="px-4 py-3 text-left">Stats</th>
            <th className="px-4 py-3 text-left min-w-[120px]">Show Logs</th>
          </tr>
        </thead>
        <tbody>
          {mockBack.map((server) => (
            <BoardItem key={server.id} server={server} />
          ))}
        </tbody>
      </table>
    </div>
  );
}
