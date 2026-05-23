import { useState } from "react";
import { Search, Eye } from "lucide-react";
import { useTableFetch } from "@/hooks/useTableFetch";
import { DataTable } from "@/components/ui/DataTable";

// Define the shape of your driver data based on your .NET API
interface Driver {
    id: string;
    fullName: string;
    cin: string;
    licenseNumber: string;
    submissionDate: string;
    dossierStatus: "SUBMITTED" | "UNDER_REVIEW";
}

export function PendingDriversTable() {
    const [search, setSearch] = useState("");

    const { data, isLoading, page, totalPages, setPage } = useTableFetch<Driver>({
        endpoint: "/endpoint/drivers/DriversRequest",
        filters: {
            search,
        }
    });

    const columns = [
        { header: "Full Name", render: (row: Driver) => <span className="font-medium text-foreground">{row.fullName}</span> },
        { header: "CIN", render: (row: Driver) => <span className="text-muted-foreground">{row.cin}</span> },
        { header: "Driver's License Number", render: (row: Driver) => <span className="text-muted-foreground">{row.licenseNumber}</span> },
        {
            header: "Submission Date", render: (row: Driver) => (
                <span className="text-muted-foreground">
                    {new Date(row.submissionDate).toLocaleDateString('mor-MA')}
                </span>
            )
        },
        {
            header: "Status", render: (row: Driver) => (
                <span className={`px-2.5 py-1 rounded-full text-xs font-medium ${row.dossierStatus === 'SUBMITTED'
                    ? 'bg-blue-100 text-blue-700'
                    : 'bg-yellow-100 text-yellow-700'
                    }`}>
                    {row.dossierStatus === 'SUBMITTED' ? 'Submitted' : 'Under review'}
                </span>
            )
        },
        {
            header: "Actions", alignRight: true, render: (row: Driver) => (
                <a
                    href={`/admin/requests/${row.id}`}
                    className="inline-flex items-center justify-center gap-2 px-3 py-1.5 bg-primary text-primary-foreground rounded-md text-xs font-semibold hover:bg-primary/90 transition-colors"
                >
                    <Eye className="h-3.5 w-3.5" />
                    Review dossier
                </a>
            )
        }
    ];

    return (
        <div className="bg-card rounded-xl shadow-sm border p-4 space-y-4 w-full">
            {/* Header & Search */}
            <div className="flex justify-between items-center">
                <h2 className="text-lg font-bold text-foreground">Drivers Requests</h2>
                <div className="relative w-72">
                    <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
                    <input
                        type="text"
                        placeholder="Search by name..."
                        className="pl-9 pr-4 py-2 w-full border rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-primary"
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                    />
                </div>
            </div>

            {/* The Table */}
            <DataTable
                columns={columns}
                data={data}
                isLoading={isLoading}
                page={page}
                totalPages={totalPages}
                onPageChange={setPage}
            />
        </div>
    );
}