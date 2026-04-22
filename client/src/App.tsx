import { useState } from "react";
import { useAuth } from "./context/AuthContext";
import { AppSidebar } from "@/components/AppSidebar";
import Header from "@/components/Header";
import ShortenerForm from "@/components/ShortenerForm";
import LoginModal from "@/modals/LoginModal";
import RegisterModal from "@/modals/RegisterModal";
import ViewAllModal from "@/modals/ViewAllModal";
import CustomLinkModal from "@/modals/CustomLinkModal";
import DeleteModal from "@/modals/DeleteModal";
import ErrorModal from "@/modals/ErrorModal";
import {api} from "./api/client.ts";

interface ShortLink {
    id: string;
    name: string | null;
    shortUrl: string;
    redirectUrl: string;
    description: string | null;
}

type ModalType = "login" | "register" | "viewAll" | "customLink" | null;

function App() {
    const { isAuthenticated, isLoading } = useAuth();
    const [modal, setModal] = useState<ModalType>(null);
    const [linkToDelete, setLinkToDelete] = useState<ShortLink | null>(null);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);

    function closeModal() { setModal(null); }
    function openError(msg: string) { setErrorMessage(msg); }
    function closeError() { setErrorMessage(null); }

    function handleDeleteRequest(link: ShortLink) { setLinkToDelete(link); }
    async function handleDeleteConfirm() {
        if (!linkToDelete) return;
        try {
            await api.del(`/api/Link/delete/${linkToDelete.shortUrl}`);
            setLinkToDelete(null);
        } catch (err) {
            openError((err as Error).message);
            setLinkToDelete(null);
        }
    }
    function handleDeleteClose() { setLinkToDelete(null); }

    if (isLoading) {
        return (
            <div className="min-h-screen flex items-center justify-center">
                <span className="text-sm text-muted-foreground">Loading...</span>
            </div>
        );
    }

    return (
<>
    {isAuthenticated ? (<>
            <Header onLogin={() => setModal("login")} onRegister={() => setModal("register")} />
            <AppSidebar
                onViewAll={() => setModal("viewAll")}
                onCustomLink={() => setModal("customLink")}
            />
            <main className="pl-56 pt-14 min-h-screen flex items-center justify-center p-8">
                <ShortenerForm onError={openError} />
            </main>
        </>
    ) : (
        <div className="flex flex-col min-h-screen">
            <Header onLogin={() => setModal("login")} onRegister={() => setModal("register")} />
            <div className="text-center mb-2">
                <p className="text-sm text-muted-foreground">
                    <button onClick={() => {}} className="text-primary hover:underline">Sign in</button>
                    {" "}for custom slugs and link management.
                </p>
            </div>
            <main className="flex flex-1 items-center justify-center min-h-[calc(100vh-3.5rem)] pt-14">

                <ShortenerForm onError={openError} />
            </main>
        </div>
    )}
            <LoginModal
                open={modal === "login"}
                onClose={closeModal}
                onSwitchToRegister={() => setModal("register")}
                onError={openError}
            />
            <RegisterModal
                open={modal === "register"}
                onClose={closeModal}
                onError={openError}
                onSwitchToLogin={() => setModal("login")}
            />
            <ViewAllModal
                open={modal === "viewAll"}
                onClose={closeModal}
                onError={openError}
                onDeleteRequest={handleDeleteRequest}
            />
            <CustomLinkModal
                open={modal === "customLink"}
                onClose={closeModal}
                onError={openError}
            />
            <DeleteModal
                open={linkToDelete !== null}
                link={linkToDelete}
                onError={openError}
                onClose={handleDeleteClose}
                onConfirm={handleDeleteConfirm}
            />
            <ErrorModal
                open={errorMessage !== null}
                message={errorMessage}
                onClose={closeError}
            />
    </>
);}

export default App;