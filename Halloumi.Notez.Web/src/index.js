import * as mm from '@magenta/music';

const RNN_CHECKPOINTS = 'https://storage.googleapis.com/magentadata/js/checkpoints/music_rnn/melody_rnn';
const VAE_CHECKPOINTS = 'https://storage.googleapis.com/magentadata/js/checkpoints/music_vae/mel_4bar_small_q2';

const musicRNN = new mm.MusicRNN(RNN_CHECKPOINTS);
const player = new mm.Player(false, {
    run: (note) => {
    },
    stop: () => {
        playSequence();
    }
  });

const musicVAE = new mm.MusicVAE(VAE_CHECKPOINTS);

var originalSequence;
var secondSequence;
var playerSequence; 

var isPlayerInitialized = false;

const loadButton = document.getElementById('load');
const loadSecondButton = document.getElementById('loadSecond');
const randomizeButton = document.getElementById('randomize');
const interpolateButton = document.getElementById('interpolate');
const resetButton = document.getElementById('reset');
const playButton = document.getElementById('play');
const stopButton = document.getElementById('stop');
const downloadButton = document.getElementById('download');

loadButton.addEventListener('change', loadFile);
loadSecondButton.addEventListener('change', loadSecondFile);
randomizeButton.addEventListener('click', randomizeSequence);
resetButton.addEventListener('click', resetSequence);
playButton.addEventListener('click', playSequence);
stopButton.addEventListener('click', stopSequence);
downloadButton.addEventListener('click', downloadSequence);
interpolateButton.addEventListener('click', interpolateSequences);

musicRNN.initialize().then(() => {
    loadButton.disabled = false;
});

musicVAE.initialize();

function loadFile(e) {
    if (player.isPlaying()) {
        player.stop();
    }    
    mm.blobToNoteSequence(e.target.files[0]).then((sequence) => setSequence(sequence));
}

function loadSecondFile(e) {
    mm.blobToNoteSequence(e.target.files[0]).then((sequence) => setSecondSequence(sequence));
}


function stopSequence() {
    if (player.isPlaying()) {
        player.stop();
    }
    playButton.disabled = false;
    stopButton.disabled = true;
}

function playSequence() {
    
    if(!isPlayerInitialized)  {
        mm.Player.tone.context.resume();
        isPlayerInitialized = true;
    }

    if (player.isPlaying()) {
        player.stop();
    }
    
    var tempo = 60;
    if(playSequence.tempos && playSequence.tempos.length)
        tempo = playerSequence.tempos[0].qpm;
    if(tempo == 120)
        tempo = 60;
    player.setTempo(tempo);

    player.start(playerSequence);
    
    playButton.disabled = true;
    stopButton.disabled = false;
}

function setSequence(sequence) {
    console.log(sequence);
    originalSequence = mm.sequences.quantizeNoteSequence(sequence, 8);
    console.log(originalSequence);

    playerSequence = originalSequence;


    resetButton.disabled = false;
    randomizeButton.disabled = false;    
    downloadButton.disabled = false;
    loadSecondButton.disabled = false;

    playSequence();
}

function setSecondSequence(sequence) {
    secondSequence = mm.sequences.quantizeNoteSequence(sequence, 8);
    interpolateButton.disabled = false;
}

function resetSequence() {
    playerSequence = originalSequence;
    playSequence();
}

function interpolateSequences() {
    var numInterpolations = 3;
    musicVAE.interpolate([originalSequence, secondSequence], numInterpolations).then((interpolations) => {

        playerSequence = interpolations[1];
        playButton.disabled = false;
        playSequence();

    });
}


function randomizeSequence() {
    const steps = 32 * 2;
    const temperature = 1.25;    
    
    playButton.disabled = true;
    musicRNN.continueSequence(originalSequence, steps, temperature).then((continuedSequence) => {
        playerSequence = continuedSequence;
        playButton.disabled = false;
        playSequence();
    });
}

function downloadSequence() {
    
        var filename = 'random.mid'
        var data = mm.sequenceProtoToMidi(playerSequence, 60)
        var blob = new Blob([data], {type: 'text/csv'});
        if(window.navigator.msSaveOrOpenBlob) {
            window.navigator.msSaveBlob(blob, filename);
        }
        else {
            var elem = window.document.createElement('a');
            elem.href = window.URL.createObjectURL(blob);
            elem.download = filename;        
            document.body.appendChild(elem);
            elem.click();        
            document.body.removeChild(elem);
        }
    
}
