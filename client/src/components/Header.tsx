import {Button} from "../../components/ui/button.tsx";

function Header () {
    function openRegisterDialog() {

    }

    function openLoginDialog() {

    }

    return (
        <header>
            <div className="columns-2 columns"><h1>LinkShort</h1></div>
            <div className="columns-8 columns"></div>
            <div className="column-end-2">
                <Button onClick={()=> openLoginDialog()}>Login</Button>
                <Button onClick={()=> openRegisterDialog()}>Sign up</Button>
            </div>

        </header>

)
}

export default Header