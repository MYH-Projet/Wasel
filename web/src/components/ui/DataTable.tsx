import { ChevronLeft, ChevronRight } from "lucide-react";

// Generic types so it works with ANY data
interface Column<T> {
    header: string;
    render: (row: T) => React.ReactNode;
    alignRight?: boolean;
}

interface DataTableProps<T> {
    columns: Column<T>[];
    data: T[];
    isLoading: boolean;
    page: number;
    totalPages: number;
    onPageChange: (newPage: number) => void;
}

export function DataTable<T>({ columns, data, isLoading, page, totalPages, onPageChange }: DataTableProps<T>) {
    return (
        <div className="space-y-4">
            <div className="overflow-x-auto border rounded-md bg-white">
                <table className="w-full text-left text-sm whitespace-nowrap">
                    <thead className="bg-slate-50 border-b text-slate-500">
                        <tr>
                            {columns.map((col, idx) => (
                                <th key={idx} className={`px-4 py-3 font-medium ${col.alignRight ? 'text-right' : ''}`}>
                                    {col.header}
                                </th>
                            ))}
                        </tr>
                    </thead>
                    <tbody className="divide-y">
                        {isLoading ? (
                            <tr><td colSpan={columns.length} className="px-4 py-8 text-center text-slate-500">Chargement...</td></tr>
                        ) : data.length === 0 ? (
                            <tr><td colSpan={columns.length} className="px-4 py-8 text-center text-slate-500">Aucune donnée trouvée.</td></tr>
                        ) : (
                            data.map((row, rowIndex) => (
                                <tr key={rowIndex} className="hover:bg-slate-50">
                                    {columns.map((col, colIndex) => (
                                        <td key={colIndex} className={`px-4 py-3 ${col.alignRight ? 'text-right' : ''}`}>
                                            {col.render(row)}
                                        </td>
                                    ))}
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>

            {/* Reusable Pagination */}
            <div className="flex items-center justify-between pt-2">
                <div className="text-sm text-slate-500">Page {page} sur {totalPages}</div>
                <div className="flex gap-2">
                    <button
                        onClick={() => onPageChange(Math.max(1, page - 1))}
                        disabled={page === 1 || isLoading}
                        className="p-2 border rounded-md hover:bg-slate-50 disabled:opacity-50"
                    >
                        <ChevronLeft className="h-4 w-4" />
                    </button>
                    <button
                        onClick={() => onPageChange(Math.min(totalPages, page + 1))}
                        disabled={page === totalPages || isLoading || totalPages === 0}
                        className="p-2 border rounded-md hover:bg-slate-50 disabled:opacity-50"
                    >
                        <ChevronRight className="h-4 w-4" />
                    </button>
                </div>
            </div>
        </div>
    );
}