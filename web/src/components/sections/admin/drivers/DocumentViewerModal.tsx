import { useState, useEffect } from "react";
import { Eye, X, Loader2, FileWarning } from "lucide-react";

interface Props {
    objectKey: string;
    documentType: string;
}

export function DocumentViewerModal({ objectKey, documentType }: Props) {
    const [isOpen, setIsOpen] = useState(false);
    const [docUrl, setDocUrl] = useState<string | null>(null);
    const [isPdf, setIsPdf] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const loadDocument = async () => {
        setIsLoading(true);
        setError(null);
        setDocUrl(null);

        try {
            // 1. Fetch pre-signed view URL from our Astro endpoint
            const res = await fetch(`/endpoint/files?objectKey=${encodeURIComponent(objectKey)}`);
            if (!res.ok) {
                throw new Error("Impossible d'obtenir l'URL de téléchargement");
            }
            
            const data = await res.json();
            const viewUrl = data.viewUrl;

            if (!viewUrl) {
                throw new Error("L'URL du document est vide");
            }

            // 2. Fetch the actual file binary (image/pdf) from MinIO
            const fileRes = await fetch(viewUrl);
            if (!fileRes.ok) {
                throw new Error("Le fichier est introuvable dans le stockage (MinIO)");
            }

            const blob = await fileRes.blob();
            const objectUrl = URL.createObjectURL(blob);
            
            // Check if the file is a PDF
            setIsPdf(blob.type === "application/pdf" || objectKey.toLowerCase().endsWith(".pdf"));
            setDocUrl(objectUrl);
        } catch (err: any) {
            console.error(err);
            setError(err.message || "Une erreur est survenue lors de la récupération du fichier.");
        } finally {
            setIsLoading(false);
        }
    };

    const handleOpen = () => {
        setIsOpen(true);
        loadDocument();
    };

    const handleClose = () => {
        setIsOpen(false);
        if (docUrl) {
            URL.revokeObjectURL(docUrl);
            setDocUrl(null);
        }
    };

    // Cleanup object URL on unmount
    useEffect(() => {
        return () => {
            if (docUrl) {
                URL.revokeObjectURL(docUrl);
            }
        };
    }, [docUrl]);

    return (
        <>
            {/* The Trigger Button */}
            <button
                onClick={handleOpen}
                className="inline-flex items-center justify-end gap-1 text-yellow-600 font-semibold hover:text-yellow-700 transition-colors"
            >
                <Eye className="w-4 h-4" /> View
            </button>

            {/* The Modal Overlay */}
            {isOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/65 backdrop-blur-sm">
                    {/* The Modal Box */}
                    <div className="bg-card rounded-xl shadow-2xl max-w-4xl w-full overflow-hidden animate-in fade-in zoom-in duration-200 border">
                        {/* Modal Header */}
                        <div className="flex justify-between items-center p-4 border-b">
                            <h3 className="font-bold text-lg text-foreground">{documentType}</h3>
                            <button
                                onClick={handleClose}
                                className="p-1.5 text-muted-foreground hover:bg-muted rounded-md transition-colors"
                            >
                                <X className="w-5 h-5" />
                            </button>
                        </div>

                        {/* Modal Content Area */}
                        <div className="p-6 bg-muted flex justify-center items-center min-h-[400px]">
                            {isLoading && (
                                <div className="flex flex-col items-center gap-3 text-muted-foreground">
                                    <Loader2 className="w-8 h-8 animate-spin text-yellow-600" />
                                    <p className="text-sm">Chargement du document depuis le stockage...</p>
                                </div>
                            )}

                            {error && (
                                <div className="flex flex-col items-center gap-3 text-red-500 max-w-md text-center">
                                    <FileWarning className="w-12 h-12 text-red-400" />
                                    <p className="font-semibold">Erreur de chargement</p>
                                    <p className="text-xs text-muted-foreground">{error}</p>
                                    <button 
                                        onClick={loadDocument} 
                                        className="mt-2 px-4 py-2 bg-yellow-600 hover:bg-yellow-700 text-white text-xs font-semibold rounded-lg transition-colors"
                                    >
                                        Réessayer
                                    </button>
                                </div>
                            )}

                            {!isLoading && !error && docUrl && (
                                isPdf ? (
                                    <iframe
                                        src={docUrl}
                                        className="w-full h-[60vh] rounded-md border bg-white"
                                        title={documentType}
                                    />
                                ) : (
                                    <img
                                        src={docUrl}
                                        alt={documentType}
                                        className="max-h-[60vh] object-contain rounded-md shadow-sm border bg-card"
                                    />
                                )
                            )}
                        </div>
                    </div>
                </div>
            )}
        </>
    );
}