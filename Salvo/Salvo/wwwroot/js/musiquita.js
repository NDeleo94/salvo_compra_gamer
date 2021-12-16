const cargarSonido = function (fuente) {
    const sonido = document.createElement("audio");
    sonido.src = fuente;
    sonido.loop = true;
    sonido.setAttribute("preload", "auto");
    sonido.setAttribute("controls", "none");
    sonido.style.display = "none"; // <-- oculto
    document.body.appendChild(sonido);
    return sonido;
};

const $botonReproducir = document.querySelector("#btnReproducir"),
    $botonPausar = document.querySelector("#btnPausar"),
    $botonReiniciar = document.querySelector("#btnReiniciar");
	
// El sonido que podemos reproducir o pausar
const sonido = cargarSonido("music/Battleship.mp3");

let playing = 0;

$botonReproducir.onclick = () => {
	if(playing == 0){
		sonido.play();
    sonido.muted = false;
		playing = 1;
	}
	else{
		//sonido.pause();
    sonido.muted = true;
		playing = 0;
	}
	
	FbotonOn();
};
$botonPausar.onclick = () => {
    sonido.pause();
};
$botonReiniciar.onclick = () => {
    sonido.currentTime = 0;
};

// function FbotonOn() {
  // var uno = document.getElementById('btnReproducir');
  // if (uno.innerText == 'off') 
  		// uno.innerText = 'on';
  // else uno.innerText = 'off'; 
// }

function FbotonOn() {
  var imagen = document.getElementById('dalePlay');
  if (playing == 0){
	  imagen.src="img/mute.png";
  }
  else imagen.src="img/play.png"; 
}