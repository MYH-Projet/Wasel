import { useState } from "react";
import { Search, Filter, Eye, ShieldBan, ShieldCheck, AlertCircle } from "lucide-react";
import { useTableFetch } from "@/hooks/useTableFetch";
import { DataTable } from "@/components/ui/DataTable";
import { format } from "date-fns";
import { toast } from "sonner";

interface UserData {
    id: string;
    fullName: string;
    email: string;
    phone: string;
    status: "ACTIVE" | "INACTIVE" | "BLOCKED";
    activeRole: "CLIENT" | "DRIVER" | "DISPATCHER";
    createdAt: string;
}

export function AllUsersTable() {
    const [search, setSearch] = useState("");
    const [statusFilter, setStatusFilter] = useState("");
    const [roleFilter, setRoleFilter] = useState("");

    // Block/Unblock Modal State
    const [actionModalData, setActionModalData] = useState<{
        isOpen: boolean;
        userId: string | null;
        userName: string | null;
        currentStatus: string
    }>({ isOpen: false, userId: null, userName: null, currentStatus: "" });
    const [isProcessing, setIsProcessing] = useState(false);

    // Fetch Hook
    const { data, isLoading, page, totalPages, setPage, setData } = useTableFetch<UserData>({
        endpoint: "/endpoint/users",
        filters: { search, status: statusFilter, role: roleFilter }
    });

    const handleToggleUserStatus = async () => {
        setIsProcessing(true);
        const isBlocking = actionModalData.currentStatus !== "BLOCKED";
        const newStatus = isBlocking ? "BLOCKED" : "ACTIVE";

        try {
            // Optimistically update the table data without full reload
            const res = await fetch("/endpoint/users", {
                method: "PATCH",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    id: actionModalData.userId,
                    status: newStatus
                })
            });
            if (!res.ok) {
                toast.error(`Failed to ${isBlocking ? 'block' : 'unblock'} user.`);
                return;
            }


            setData(data.map((user) => {
                if (user.id === actionModalData.userId) {
                    return {
                        ...user,
                        status: newStatus
                    };
                }
                return user;
            }))
            toast.success(`User successfully ${isBlocking ? 'blocked' : 'unblocked'}.`);



            setActionModalData({ isOpen: false, userId: null, userName: null, currentStatus: "" });
        } catch (error) {
            toast.error(`Failed to ${isBlocking ? 'block' : 'unblock'} user.`);
        } finally {
            setIsProcessing(false);
        }
    };

    const getStatusBadge = (status: string) => {
        const styles: Record<string, string> = {
            ACTIVE: "bg-green-100 text-green-700",
            INACTIVE: "bg-slate-100 text-slate-700",
            BLOCKED: "bg-red-100 text-red-700",
        };
        return <span className={`px-2 py-1 text-[11px] font-bold rounded-md uppercase tracking-wider ${styles[status] || styles.INACTIVE}`}>{status}</span>;
    };

    const getRoleBadge = (role: string) => {
        const styles: Record<string, string> = {
            CUSTOMER: "bg-blue-50 text-blue-700 border-blue-200",
            DRIVER: "bg-yellow-50 text-yellow-700 border-yellow-200",
        };
        return <span className={`px-2 py-1 text-xs font-semibold rounded border ${styles[role] || "bg-slate-50"}`}>{role}</span>;
    };

    const columns = [
        { header: "Name", render: (row: UserData) => <span className="font-bold text-slate-900">{row.fullName}</span> },
        {
            header: "Contact",
            render: (row: UserData) => (
                <div className="flex flex-col text-xs text-slate-500">
                    <span>{row.email}</span>
                    <span>{row.phone}</span>
                </div>
            )
        },
        { header: "Role", render: (row: UserData) => getRoleBadge(row.activeRole) },
        { header: "Status", render: (row: UserData) => getStatusBadge(row.status) },
        {
            header: "Joined Date",
            render: (row: UserData) => <span className="text-slate-600 text-sm">{format(new Date(row.createdAt), "MMM dd, yyyy")}</span>
        },
        {
            header: "Actions", alignRight: true, render: (row: UserData) => {
                const isBlocked = row.status === "BLOCKED";
                return (
                    <div className="flex gap-2 justify-end items-center">
                        <button
                            onClick={() => setActionModalData({ isOpen: true, userId: row.id, userName: row.fullName, currentStatus: row.status })}
                            className={`p-1.5 rounded-md transition-colors ${isBlocked
                                ? "text-green-600 hover:bg-green-50"
                                : "text-red-500 hover:bg-red-50"
                                }`}
                            title={isBlocked ? "Unblock User" : "Block User"}
                        >
                            {isBlocked ? <ShieldCheck className="w-4 h-4" /> : <ShieldBan className="w-4 h-4" />}
                        </button>
                    </div>
                )
            }
        }
    ];

    const isBlockingModal = actionModalData.currentStatus !== "BLOCKED";

    return (
        <div className="bg-white rounded-xl shadow-sm border p-4 space-y-4">

            {/* Filters Toolbar */}
            <div className="flex flex-col xl:flex-row justify-between gap-4">
                <div className="relative w-full xl:w-80">
                    <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-slate-500" />
                    <input
                        type="text"
                        placeholder="Search by name or email..."
                        className="pl-9 pr-4 py-2 w-full border rounded-md text-sm outline-none focus:border-slate-400"
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                    />
                </div>

                <div className="flex flex-wrap items-center gap-3">

                    {/* Role Filter */}
                    <div className="flex items-center gap-2 bg-slate-50 border rounded-md px-3 py-1">
                        <Filter className="w-4 h-4 text-slate-500" />
                        <select
                            className="bg-transparent border-none text-sm outline-none focus:ring-0 cursor-pointer"
                            value={roleFilter}
                            onChange={(e) => setRoleFilter(e.target.value)}
                        >
                            <option value="">All Roles</option>
                            <option value="CLIENT">Clients</option>
                            <option value="DRIVER">Drivers</option>
                        </select>
                    </div>

                    {/* Status Filter */}
                    <div className="flex items-center gap-2 bg-slate-50 border rounded-md px-3 py-1">
                        <select
                            className="bg-transparent border-none text-sm outline-none focus:ring-0 cursor-pointer pl-1"
                            value={statusFilter}
                            onChange={(e) => setStatusFilter(e.target.value)}
                        >
                            <option value="">All Statuses</option>
                            <option value="ACTIVE">Active</option>
                            <option value="INACTIVE">Inactive</option>
                            <option value="BLOCKED">Blocked</option>
                        </select>
                    </div>
                </div>
            </div>

            {/* The Generic Table */}
            <DataTable columns={columns} data={data} isLoading={isLoading} page={page} totalPages={totalPages} onPageChange={setPage} />

            {/* Block / Unblock Modal */}
            {actionModalData.isOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
                    <div className="bg-white p-6 rounded-xl shadow-xl w-full max-w-md animate-in fade-in zoom-in duration-200">
                        <h4 className={`text-lg font-bold flex items-center gap-2 ${isBlockingModal ? 'text-red-600' : 'text-green-600'}`}>
                            <AlertCircle className="w-5 h-5" />
                            {isBlockingModal ? 'Block User Account' : 'Unblock User Account'}
                        </h4>
                        <p className="text-sm text-slate-600 mt-2">
                            Are you sure you want to {isBlockingModal ? 'block' : 'unblock'} user <span className="font-bold text-slate-900">{actionModalData.userName}</span>?
                            {isBlockingModal ? " They will immediately lose access to the platform." : " They will regain full access to the platform."}
                        </p>

                        <div className="mt-6 flex justify-end gap-3">
                            <button
                                onClick={() => setActionModalData({ isOpen: false, userId: null, userName: null, currentStatus: "" })}
                                disabled={isProcessing}
                                className="px-4 py-2 text-sm font-medium text-slate-600 hover:bg-slate-100 rounded-md transition-colors"
                            >
                                Cancel
                            </button>
                            <button
                                onClick={handleToggleUserStatus}
                                disabled={isProcessing}
                                className={`px-4 py-2 text-sm font-medium text-white rounded-md disabled:opacity-50 transition-colors ${isBlockingModal ? 'bg-red-600 hover:bg-red-700' : 'bg-green-600 hover:bg-green-700'
                                    }`}
                            >
                                {isProcessing ? "Processing..." : (isBlockingModal ? "Confirm Block" : "Confirm Unblock")}
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}