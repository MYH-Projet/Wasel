import { useState, useEffect } from "react";
import { Loader2, FileWarning } from "lucide-react";

interface Props {
    objectKey: string;
    documentType: string;
}

export function DocumentViewerInline({ objectKey, documentType }: Props) {
    const [docUrl, setDocUrl] = useState<string | null>(null);
    const [isPdf, setIsPdf] = useState(false);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const loadDocument = async () => {
        setIsLoading(true);
        setError(null);
        setDocUrl(null);

        try {
            const res = await fetch(`/endpoint/files?objectKey=${encodeURIComponent(objectKey)}`);
            if (!res.ok) {
                throw new Error("Impossible d'obtenir l'URL de téléchargement");
            }
            
            const data = await res.json();
            const viewUrl = data.viewUrl;

            if (!viewUrl) {
                throw new Error("L'URL du document est vide");
            }

            // Fetch the actual file binary
            const fileRes = await fetch(viewUrl);
            if (!fileRes.ok) {
                throw new Error("Le fichier est introuvable dans le stockage");
            }

            const blob = await fileRes.blob();
            const objectUrl = URL.createObjectURL(blob);
            
            setIsPdf(blob.type === "application/pdf" || objectKey.toLowerCase().endsWith(".pdf"));
            setDocUrl(objectUrl);
        } catch (err: any) {
            console.error(err);
            setError(err.message || "Erreur de chargement");
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        loadDocument();
        return () => {
            if (docUrl) {
                URL.revokeObjectURL(docUrl);
            }
        };
    }, [objectKey]);

    if (isLoading) {
        return (
            <div className="flex flex-col items-center justify-center p-12 bg-muted rounded-md border text-muted-foreground min-h-[300px]">
                <Loader2 className="w-8 h-8 animate-spin text-yellow-600 mb-2" />
                <p className="text-sm">Chargement du document...</p>
            </div>
        );
    }

    if (error) {
        return (
            <div className="flex flex-col items-center justify-center p-12 bg-muted rounded-md border text-red-500 min-h-[300px]">
                <FileWarning className="w-10 h-10 mb-2 text-red-400" />
                <p className="font-semibold text-sm">Erreur</p>
                <p className="text-xs text-muted-foreground">{error}</p>
                <button 
                    onClick={loadDocument} 
                    className="mt-4 px-4 py-2 bg-yellow-600 hover:bg-yellow-700 text-white text-xs font-semibold rounded-lg transition-colors"
                >
                    Réessayer
                </button>
            </div>
        );
    }

    if (!docUrl) return null;

    return (
        <div className="w-full">
            {isPdf ? (
                <iframe
                    src={docUrl}
                    className="w-full h-[500px] rounded-md border bg-white"
                    title={documentType}
                />
            ) : (
                <img
                    src={docUrl}
                    alt={documentType}
                    className="w-full max-h-[500px] object-contain rounded-md shadow-sm border bg-card"
                />
            )}
        </div>
    );
}
