import React, { useState } from "react";
import { toast } from "sonner";
import { Modal } from "@/components/ui/Modal";

interface Props {
    deliveryId: string;
    status: string;
}

export const DeliveryActionButtons: React.FC<Props> = ({ deliveryId, status }) => {
    const [isCancelModalOpen, setIsCancelModalOpen] = useState(false);
    const [cancelReason, setCancelReason] = useState("");
    const [isCancelling, setIsCancelling] = useState(false);

    const handlePrint = () => {
        window.print();
    };

    const handleCancel = async () => {
        if (!cancelReason.trim()) {
            toast.error("Please provide a reason for cancellation");
            return;
        }

        setIsCancelling(true);
        try {
            const response = await fetch("/endpoint/deliveries", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ id: deliveryId, reason: cancelReason }),
            });

            if (!response.ok) {
                const error = await response.json();
                toast.error(error.message || "Failed to cancel delivery.");
                setIsCancelling(false);
                return;
            }

            toast.success(`Delivery ${deliveryId} cancelled successfully.`);
            setIsCancelModalOpen(false);

            // Reload the page to reflect the new state (e.g., status change)
            setTimeout(() => {
                window.location.reload();
            }, 1000);

        } catch (error) {
            toast.error("An unexpected error occurred.");
        } finally {
            setIsCancelling(false);
        }
    };

    return (
        <>
            <div className="flex gap-3">
                <button
                    onClick={handlePrint}
                    className="px-4 py-2 bg-secondary text-secondary-foreground hover:bg-secondary/90 transition-colors rounded-md font-medium"
                >
                    Print Manifest
                </button>
                {!(!status || status.toUpperCase().includes("CANCEL") || status.toUpperCase().includes("DELIVERED") || status.toUpperCase().includes("RETURN")) && <button
                    onClick={() => setIsCancelModalOpen(true)}
                    className="px-4 py-2 bg-primary text-primary-foreground font-bold hover:bg-primary/90 transition-colors rounded-md"
                >
                    Cancel Delivery
                </button>}
            </div>

            <Modal
                isOpen={isCancelModalOpen}
                onClose={() => !isCancelling && setIsCancelModalOpen(false)}
                title="Cancel Delivery"
            >
                <div className="space-y-4">
                    <p className="text-sm text-muted-foreground">
                        Are you sure you want to cancel delivery <strong>{deliveryId}</strong>? This action cannot be undone.
                    </p>
                    <div>
                        <label className="block text-sm font-medium text-foreground mb-1">
                            Reason for cancellation *
                        </label>
                        <textarea
                            value={cancelReason}
                            onChange={(e) => setCancelReason(e.target.value)}
                            className="w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50 min-h-[100px]"
                            placeholder="Enter reason..."
                            disabled={isCancelling}
                        />
                    </div>
                    <div className="flex justify-end gap-3 mt-6">
                        <button
                            className="px-4 py-2 rounded-lg text-sm font-medium text-muted-foreground hover:bg-muted/50 transition-colors"
                            onClick={() => setIsCancelModalOpen(false)}
                            disabled={isCancelling}
                        >
                            Back
                        </button>
                        <button
                            className="px-4 py-2 rounded-lg text-sm font-bold bg-destructive text-destructive-foreground hover:bg-destructive/90 transition-colors disabled:opacity-50"
                            onClick={handleCancel}
                            disabled={isCancelling}
                        >
                            {isCancelling ? "Cancelling..." : "Confirm Cancellation"}
                        </button>
                    </div>
                </div>
            </Modal>
        </>
    );
};
