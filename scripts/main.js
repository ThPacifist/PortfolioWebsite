class MyHeader extends HTMLElement 
{
    connectedCallback() 
    {
        this.innerHTML = `
        <header>
        <nav class="navbar navbar-expand-lg navbar-dark p-0">
            <div class="container-fluid">
              <!-- a class="navbar-brand" href="#">Navbar</a> -->

              <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNavDropdown" aria-controls="navbarNavDropdown" aria-expanded="false" aria-label="Toggle navigation">
                <span class="navbar-toggler-icon"></span>
              </button> <!-- This is the burger image that appears when the screen gets smaller-->

              <div class="collapse navbar-collapse" id="navbarNavDropdown">
                <ul class="navbar-nav">

                  <li class="nav-item border-end border-3 border-dark ">
                    <a class="nav-link" href="index.html">Home</a>
                  </li>

                  <li class="nav-item dropdown border-end border-3 border-dark">
                    <a class="nav-link dropdown-toggle" href="#" id="navbarDropdownMenuLink" role="button" data-bs-toggle="dropdown" aria-expanded="false">
                      Projects
                    </a>
                    <ul class="dropdown-menu dropdown-menu-dark" aria-labelledby="navbarDropdownMenuLink">
                      <li><a class="dropdown-item fs-4" href="embodiment.html">Embodiment</a></li>
                      <li><a class="dropdown-item fs-4" href="">3 O'Clock Horror</a></li> <!--Link: 3oclockhorror.html -->
                    </ul>
                  </li>

                  <li class="nav-item border-end border-3 border-dark">
                    <a class="nav-link" href="documents/Game Developer Resume.pdf" target="_blank">Resume</a>
                  </li>

                  <li class="nav-item border-end border-3 border-dark">
                    <a class="nav-link" href="aboutme.html">About Me</a>
                  </li>
                </ul>
              </div>
            </div>
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
              <div class="container-xl">
                <div class="row">
                  <div class="col-6 border-end border-3 border-black pt-2 pb-1">
                    <a href="mailto:benjamin.henschen@gmail.com" class="mx-3"><img src="pictures/gmail.png" style="height: 32px"/></a>
                    <a href="https://www.linkedin.com/in/benjamin-henschen/" class="mx-3"><img src="pictures/linkedin.png" style="height: 32px"/></a>
                    <a href="https://github.com/ThPacifist" class="mx-3"><img src="pictures/github.png" style="height: 32px"/></a>
                    <a href="https://www.youtube.com/channel/UCiC15eythbplhk4lK41BqMg?view_as=subscriber" class="mx-3"><img src="pictures/youtube.png" style="height: 32px" /></a>
                  </div>
                  <div class="col-6 pt-2">
                    <p class="fs-5"> Created by Benjamin Henschen</p>
                  </div>
                </div>
              </div>
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