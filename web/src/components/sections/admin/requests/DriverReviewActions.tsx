import { useState } from "react";
import { CheckCircle, XCircle, Clock, AlertTriangle } from "lucide-react";
import { toast } from "sonner";

interface Props {
    driverId: string;
    currentStatus: string;
}

export function DriverReviewActions({ driverId, currentStatus }: Props) {
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [showApproveModal, setShowApproveModal] = useState(false);
    const [showRejectModal, setShowRejectModal] = useState(false);
    const [rejectionReason, setRejectionReason] = useState("");

    const handleAction = async (endpoint: string, payload?: any) => {
        setIsSubmitting(true);
        try {
            const response = await fetch(`/endpoint/requests/DriversRequest`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    id: driverId,
                    action: endpoint,
                    ...payload
                })
            });

            if (response.ok) {
                toast.success(`Operation seccesufuly`);
                // Retour automatique à la liste après un court délai pour voir le toast
                setTimeout(() => {
                    window.location.href = "/admin/requests";
                }, 1000);
            } else {
                const errorData = await response.json();
                toast.error(errorData.message || "error in operation");
                setIsSubmitting(false);
            }
        } catch (error) {
            toast.error("error in connection with server.");
            setIsSubmitting(false);
        }
    };

    return (
        <div className="space-y-4">
            <h3 className="font-semibold text-slate-800">Decision</h3>

            <div className="flex flex-col gap-3">
                {/* Bouton Approuver */}
                <button
                    onClick={() => setShowApproveModal(true)}
                    disabled={isSubmitting}
                    className="w-full flex items-center justify-center gap-2 px-4 py-2 bg-green-600 text-white rounded-md font-medium hover:bg-green-700 transition"
                >
                    <CheckCircle className="w-4 h-4" /> Approve the file
                </button>

                {/* Bouton Mettre en révision */}
                {currentStatus !== "UNDER_REVIEW" && (
                    <button
                        onClick={() => handleAction("under-review")}
                        disabled={isSubmitting}
                        className="w-full flex items-center justify-center gap-2 px-4 py-2 bg-blue-100 text-blue-700 rounded-md font-medium hover:bg-blue-200 transition"
                    >
                        <Clock className="w-4 h-4" /> Put in review
                    </button>
                )}

                {/* Bouton Rejeter */}
                <button
                    onClick={() => setShowRejectModal(true)}
                    disabled={isSubmitting}
                    className="w-full flex items-center justify-center gap-2 px-4 py-2 border-2 border-red-200 text-red-600 rounded-md font-medium hover:bg-red-50 transition"
                >
                    <XCircle className="w-4 h-4" /> Reject the file
                </button>
            </div>

            {/* Modale d'Approbation */}
            {showApproveModal && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
                    <div className="bg-white p-6 rounded-lg shadow-xl w-full max-w-md">
                        <h4 className="text-lg font-bold flex items-center gap-2"><CheckCircle className="text-green-600" /> Confirm the approval</h4>
                        <p className="text-slate-600 mt-2">Are you sure you want to approve this driver ? He will receive immediate access to the platform.</p>
                        <div className="mt-6 flex justify-end gap-3">
                            <button onClick={() => setShowApproveModal(false)} className="px-4 py-2 text-slate-600 hover:bg-slate-100 rounded-md">Cancel</button>
                            <button onClick={() => handleAction("approve")} className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700">Yes, Approve</button>
                        </div>
                    </div>
                </div>
            )}

            {/* Modale de Rejet */}
            {showRejectModal && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
                    <div className="bg-white p-6 rounded-lg shadow-xl w-full max-w-md">
                        <h4 className="text-lg font-bold flex items-center gap-2"><AlertTriangle className="text-red-600" /> Confirm the reject</h4>
                        <p className="text-slate-600 mt-2 mb-4">Please enter the reason for rejection. This message will be sent to the driver.</p>

                        <textarea
                            className="w-full border rounded-md p-2 text-sm focus:ring-2 focus:ring-red-500 outline-none"
                            rows={4}
                            placeholder="Ex: The insurance copy is illegible..."
                            value={rejectionReason}
                            onChange={(e) => setRejectionReason(e.target.value)}
                        />

                        <div className="mt-6 flex justify-end gap-3">
                            <button onClick={() => setShowRejectModal(false)} className="px-4 py-2 text-slate-600 hover:bg-slate-100 rounded-md">Annuler</button>
                            <button
                                onClick={() => handleAction("reject", { reason: rejectionReason })}
                                disabled={rejectionReason.trim().length < 10}
                                className="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 disabled:opacity-50"
                            >
                                Reject
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}