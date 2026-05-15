import { useState, useEffect } from "react";
import { Search, ChevronLeft, ChevronRight, Eye } from "lucide-react";

// Define the shape of your driver data based on your .NET API
interface Driver {
    id: string;
    fullName: string;
    cin: string;
    licenseNumber: string;
    submissionDate: string;
    dossierStatus: "SUBMITTED" | "UNDER_REVIEW";
}


const driversRequestsData = {
    totalPages: 3,
    totalCount: 24,
    items: [
        {
            id: "REQ-8942A",
            fullName: "Youssef Alaoui",
            cin: "KB123456",
            licenseNumber: "12/345678",
            submissionDate: "2026-05-15T08:30:00Z",
            dossierStatus: "SUBMITTED",
        },
        {
            id: "REQ-8943B",
            fullName: "Fatima Zahra Mansouri",
            cin: "CD98765",
            licenseNumber: "09/112233",
            submissionDate: "2026-05-14T14:15:00Z",
            dossierStatus: "UNDER_REVIEW",
        },
        {
            id: "REQ-8944C",
            fullName: "Omar Chraibi",
            cin: "BJ554433",
            licenseNumber: "15/998877",
            submissionDate: "2026-05-14T09:00:00Z",
            dossierStatus: "SUBMITTED",
        },
        {
            id: "REQ-8945D",
            fullName: "Mehdi El Fassi",
            cin: "A12345",
            licenseNumber: "05/667788",
            submissionDate: "2026-05-13T16:45:00Z",
            dossierStatus: "UNDER_REVIEW",
        },
        {
            id: "REQ-8946E",
            fullName: "Amina Bennani",
            cin: "Z998877",
            licenseNumber: "22/445566",
            submissionDate: "2026-05-13T10:20:00Z",
            dossierStatus: "SUBMITTED",
        },
    ],
};

export function PendingDriversTable() {
    const [drivers, setDrivers] = useState<Driver[]>([]);
    const [search, setSearch] = useState("");
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        async function fetchDrivers() {
            setIsLoading(true);
            try {
                // const response = await fetch(`/endpoint/requests/DriversRequest?search=${search}&page=${page}`);
                // if (response.ok) {
                //     const newData = await response.json();
                //     setDrivers(newData.items || newData);
                //     setTotalPages(newData.totalPages || 1);
                // }
                setDrivers(driversRequestsData.items);
                setTotalPages(driversRequestsData.totalPages);
            } catch (error) {
                console.error("Fetch failed", error);
            } finally {
                setIsLoading(false);
            }
        }

        const delaySearch = setTimeout(() => {
            fetchDrivers();
        }, 300);

        return () => clearTimeout(delaySearch);
    }, [search, page]);

    // Reset to page 1 when the user types a new search
    const handleSearch = (e: React.ChangeEvent<HTMLInputElement>) => {
        setSearch(e.target.value);
        setPage(1);
    };

    return (
        <div className="bg-white rounded-xl shadow-sm border p-4 space-y-4 w-full">
            {/* Header & Search */}
            <div className="flex justify-between items-center">
                <h2 className="text-lg font-bold text-slate-800">Drivers Requests</h2>
                <div className="relative w-72">
                    <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-slate-500" />
                    <input
                        type="text"
                        placeholder="Search by name..."
                        className="pl-9 pr-4 py-2 w-full border rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-primary"
                        value={search}
                        onChange={handleSearch}
                    />
                </div>
            </div>

            {/* The Table */}
            <div className="overflow-x-auto border rounded-md">
                <table className="w-full text-left text-sm whitespace-nowrap">
                    <thead className="bg-slate-50 border-b text-slate-500">
                        <tr>
                            <th className="px-4 py-3 font-medium">Full Name</th>
                            <th className="px-4 py-3 font-medium">CIN</th>
                            <th className="px-4 py-3 font-medium">Driver's License Number</th>
                            <th className="px-4 py-3 font-medium">Submission Date</th>
                            <th className="px-4 py-3 font-medium">Status</th>
                            <th className="px-4 py-3 font-medium text-right">Actions</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y">
                        {isLoading ? (
                            <tr>
                                <td colSpan={6} className="px-4 py-8 text-center text-slate-500">
                                    Loading...
                                </td>
                            </tr>
                        ) : drivers.length === 0 ? (
                            <tr>
                                <td colSpan={6} className="px-4 py-8 text-center text-slate-500">
                                    No pending dossiers found.
                                </td>
                            </tr>
                        ) : (
                            drivers.map((driver) => (
                                <tr key={driver.id} className="hover:bg-slate-50">
                                    <td className="px-4 py-3 font-medium text-slate-900">{driver.fullName}</td>
                                    <td className="px-4 py-3 text-slate-600">{driver.cin}</td>
                                    <td className="px-4 py-3 text-slate-600">{driver.licenseNumber}</td>
                                    <td className="px-4 py-3 text-slate-600">
                                        {new Date(driver.submissionDate).toLocaleDateString('mor-MA')}
                                    </td>
                                    <td className="px-4 py-3">
                                        <span className={`px-2.5 py-1 rounded-full text-xs font-medium ${driver.dossierStatus === 'SUBMITTED'
                                            ? 'bg-blue-100 text-blue-700'
                                            : 'bg-yellow-100 text-yellow-700'
                                            }`}>
                                            {driver.dossierStatus === 'SUBMITTED' ? 'Submitted' : 'Under review'}
                                        </span>
                                    </td>
                                    <td className="px-4 py-3 text-right">
                                        {/* Link to your Document Review Portal! */}
                                        <a
                                            href={`/admin/drivers/${driver.id}`}
                                            className="inline-flex items-center justify-center gap-2 px-3 py-1.5 bg-slate-900 text-white rounded-md text-xs font-semibold hover:bg-slate-800 transition-colors"
                                        >
                                            <Eye className="h-3.5 w-3.5" />
                                            Review dossier
                                        </a>
                                    </td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>

            {/* Pagination Controls */}
            <div className="flex items-center justify-between border-t pt-4">
                <div className="text-sm text-slate-500">
                    Page {page} of {totalPages}
                </div>
                <div className="flex gap-2">
                    <button
                        onClick={() => setPage(p => Math.max(1, p - 1))}
                        disabled={page === 1 || isLoading}
                        className="p-2 border rounded-md hover:bg-slate-50 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                        <ChevronLeft className="h-4 w-4" />
                    </button>
                    <button
                        onClick={() => setPage(p => Math.min(totalPages, p + 1))}
                        disabled={page === totalPages || isLoading || totalPages === 0}
                        className="p-2 border rounded-md hover:bg-slate-50 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                        <ChevronRight className="h-4 w-4" />
                    </button>
                </div>
            </div>
        </div>
    );
}