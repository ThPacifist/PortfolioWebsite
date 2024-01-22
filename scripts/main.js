class MyHeader extends HTMLElement 
{
    connectedCallback() 
    {
        this.innerHTML = `
            <header>
                <nav id="mobile_menu"></nav>
                <nav id="nav_menu">
                    <ul>
                        <li><a href="index.html">Home</a></li>
                        <li><a href="">Projects</a>
                            <ul>
                                <li><a href="3oclockhorror.html">3 O'Clock Horror</a></li>
                                <li><a href="embodiment.html">Embodiment</a></li>
                                <li><a href="">Unnamed Samurai Game</a></li>
                                <li><a href="">Stalker and Brute</a></li>
                            </ul>
                        </li>
                        <li><a href="documents/Resume.pdf" target="_blank">Resume</a></li>
                        <li><a href="aboutme.html">About Me</a></li>
                    </ul>
                </nav>
            </header>
        `
    }
}

class MyFooter extends HTMLElement 
{
    connectedCallback() 
    {
        this.innerHTML = `
            <footer>
                <section id="links">
                    <a href="mailto:benjamin.henschen@gmail.com"><img src="pictures/gmail.png"/></a>&nbsp;&nbsp;&nbsp;&nbsp;
                    <a href="https://www.linkedin.com/in/benjamin-henschen/"><img src="pictures/linkedin.png"/></a>&nbsp;&nbsp;&nbsp;&nbsp;
                    <a href="https://github.com/ThPacifist"><img src="pictures/github.png"/></a>&nbsp;&nbsp;&nbsp;&nbsp;
                    <a href="https://www.youtube.com/channel/UCiC15eythbplhk4lK41BqMg?view_as=subscriber"><img src="pictures/youtube.png"/></a>
                </section>
            </footer>
        `
    }
}

/*
    <section id="credits">
        <h2>Credits</h2>
        <div>Icons made by <a href="https://www.flaticon.com/authors/freepik" title="Freepik">Freepik</a> from <a href="https://www.flaticon.com/" title="Flaticon">www.flaticon.com</a></div>
        <div>Icons made by <a href="https://www.flaticon.com/authors/freepik" title="Freepik">Freepik</a> from <a href="https://www.flaticon.com/" title="Flaticon">www.flaticon.com</a></div>
        <div>Icons made by <a href="https://www.flaticon.com/authors/freepik" title="Freepik">Freepik</a> from <a href="https://www.flaticon.com/" title="Flaticon">www.flaticon.com</a></div>
        <div>Icons made by <a href="https://www.flaticon.com/authors/freepik" title="Freepik">Freepik</a> from <a href="https://www.flaticon.com/" title="Flaticon">www.flaticon.com</a></div>
    </section>
*/1

customElements.define("my-header", MyHeader)
customElements.define("my-footer", MyFooter)