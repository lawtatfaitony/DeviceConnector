  
<script>
//dplayer1
    var plyerId1 = GetSelectPlayerId(1);
    const dp1 = new DPlayer({
        container: document.getElementById(plyerId1),
        screenshot: true,
        lang: languageCode,
        autoplay: true,
        mutex: true,
        video: {
            url: cameraHlsUrl,
            type: 'customHls',
            autoplay: true,
            customType: {
                customHls: function (video, player) {
                    const hls = new Hls();
                    hls.loadSource(cameraHlsUrl);
                    hls.attachMedia(video);
                },
            },
        }
    });

    // dplayer2
    var plyerId2 = GetSelectPlayerId(2);
    const dp2 = new DPlayer({
        container: document.getElementById(plyerId2),
        screenshot: true,
        lang: languageCode,
        autoplay: true,
        mutex: true,
        video: {
            url: cameraHlsUrl,
            type: 'customHls',
            autoplay: true,
            customType: {
                customHls: function (video, player) {
                    const hls = new Hls();
                    hls.loadSource(cameraHlsUrl);
                    hls.attachMedia(video);
                },
            },
        }
    });

    //dplayer3
    var plyerId3 = GetSelectPlayerId(3);
    const dp3 = new DPlayer({
        container: document.getElementById(plyerId3),
        screenshot: true,
        lang: languageCode,
        autoplay: true,
        mutex: true,
        video: {
            url: cameraHlsUrl,
            type: 'customHls',
            lang: languageCode,
            autoplay: true,
            customType: {
                customHls: function (video, player) {
                    const hls = new Hls();
                    hls.loadSource(cameraHlsUrl);
                    hls.attachMedia(video);
                },
            },
        }
    });

    //dplayer4
    var plyerId4 = GetSelectPlayerId(4);
    const dp4 = new DPlayer({
        container: document.getElementById(plyerId4),
        screenshot: true,
        lang: languageCode,
        autoplay: true,
        mutex: true,
        video: {
            url: cameraHlsUrl,
            type: 'customHls',
            autoplay: true,
            customType: {
                customHls: function (video, player) {
                    const hls = new Hls();
                    hls.loadSource(cameraHlsUrl);
                    hls.attachMedia(video);
                },
            },
        }
    });
</script>
