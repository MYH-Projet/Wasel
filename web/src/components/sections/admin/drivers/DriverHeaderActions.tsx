import { useState } from "react";
import { Mail, Ban, AlertTriangle } from "lucide-react";
import { toast } from "sonner";

interface Props {
    driverId: string;
    driverName: string;
}

export function DriverHeaderActions({ driverId, driverName }: Props) {
    // 1. Separate the states!
    const [showModal, setShowModal] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [suspendReason, setSuspendReason] = useState("");

    const handleSuspend = async () => {
        setIsSubmitting(true);
        try {
            // Call your BFF (Backend-For-Frontend) endpoint
            const response = await fetch(`/endpiont/drivers/`, {
                method: "PATCH",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    driverId: driverId,
                    action: "Blocked",
                    payload: { reason: suspendReason }
                }),
            });

            if (response.ok) {
                toast.success("Driver suspended successfully.");
                setTimeout(() => window.location.reload(), 1000);
            } else {
                toast.error("Error while suspending driver.");
                setIsSubmitting(false); // Only stop loading if it fails
            }
        } catch (error) {
            toast.error("Erreur serveur.");
            setIsSubmitting(false);
        }
    };

    const handleMessage = () => {
        toast.info("Message functionality is under development !");
    };

    return (
        <>
            <div className="flex gap-3">
                <button
                    onClick={handleMessage}
                    className="flex items-center gap-2 px-4 py-2 bg-secondary text-secondary-foreground font-medium rounded-lg hover:bg-slate-700 transition"
                >
                    <Mail className="w-4 h-4" /> Message
                </button>

                {/* 2. The trigger button is clean and simple */}
                <button
                    onClick={() => setShowModal(true)}
                    disabled={isSubmitting}
                    className="flex items-center gap-2 px-4 py-2 border border-red-200 text-red-600 font-medium rounded-lg hover:bg-red-50 transition disabled:opacity-50 disabled:cursor-not-allowed"
                >
                    <Ban className="w-4 h-4" />
                    {isSubmitting ? "Suspending..." : "Suspend"}
                </button>
            </div>

            {/* 3. The Modal lives OUTSIDE the button! */}
            {showModal && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
                    <div className="bg-card p-6 rounded-lg shadow-xl w-full max-w-md animate-in fade-in zoom-in duration-200">
                        <h4 className="text-lg font-bold flex items-center gap-2">
                            <AlertTriangle className="text-red-600" />
                            Confirm the suspend
                        </h4>
                        <p className="text-muted-foreground mt-2 mb-4">
                            Please enter the reason for suspension. This message will be sent to {driverName}.
                        </p>

                        <textarea
                            className="w-full border rounded-md p-2 text-sm focus:ring-2 focus:ring-red-500 outline-none"
                            rows={4}
                            placeholder="Ex: The insurance copy is illegible..."
                            value={suspendReason}
                            onChange={(e) => setSuspendReason(e.target.value)}
                        />

                        <div className="mt-6 flex justify-end gap-3">
                            <button
                                onClick={() => setShowModal(false)}
                                disabled={isSubmitting}
                                className="px-4 py-2 text-muted-foreground hover:bg-muted rounded-md"
                            >
                                Annuler
                            </button>
                            <button
                                onClick={handleSuspend}
                                disabled={suspendReason.trim().length < 10 || isSubmitting}
                                className="px-4 py-2 bg-red-600 text-secondary-foreground rounded-md hover:bg-red-700 disabled:opacity-50 flex items-center gap-2"
                            >
                                {isSubmitting ? "Processing..." : "Suspend definitely"}
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </>
    );
}