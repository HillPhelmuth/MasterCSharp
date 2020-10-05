window.removeMonaco = () => {
    var elem = document.querySelector('#learnCsharp');
    elem.parentNode.removeChild(elem);
}
window.removeYouTube = () => {
    var frameElement = document.querySelector('#iframe-demo');
    frameElement.parentNode.removeChild(frameElement);
    var elem = document.querySelector('#player');
    elem.parentNode.removeChild(elem);
}
window.addPlayer = () => {
    var maker = document.getElementById("makePlayer");
    var made = document.createElement("div");
    made.id = "player";
    maker.appendChild(made);
}
window.startYouTube = () => {
    var tag = document.createElement('script');
    tag.id = 'iframe-demo';
    tag.src = 'https://www.youtube.com/iframe_api';
    var firstScriptTag = document.getElementsByTagName('script')[0];
    firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);
}
window.getYouTube = (instance, video) => {
    if (typeof (YT) == 'undefined' || typeof (YT.Player) == 'undefined') {
        var tag = document.createElement('script');
        tag.id = 'iframe-demo';
        tag.src = 'https://www.youtube.com/iframe_api';
        var firstScriptTag = document.getElementsByTagName('script')[0];
        firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);
        onYouTubeIframeAPIReady();
    } else {
        onYouTubeIframeAPIReady();
    }

    var player;
    function onYouTubeIframeAPIReady() {

        player = new YT.Player('player', {
            playerVars: { 'autoplay': 1, 'controls': 1 },
            videoId: video,
            height: 400,
            width: 600,
            events: {
                'onReady': onPlayerReady,
                'onStateChange': onPlayerStateChange
            },

        });
        console.log("Api ready");
    }
    function onPlayerStateChange(event) {
        if (event.data === 0 | YT.PlayerState.ENDED) {
            console.log("Video ended");
            instance.invokeMethodAsync('GetNextVideo');

        };
        console.log("state changed");
    }

    function onPlayerReady(event) {
        event.target.playVideo();
        console.log("player ready");
    }
}
