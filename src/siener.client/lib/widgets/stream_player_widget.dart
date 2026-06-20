import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter_webrtc/flutter_webrtc.dart';
import 'package:siener.client/http_client.dart';
import 'package:siener.client/widgets/fullscreen_stream_player_widget.dart';

class StreamPlayer extends StatefulWidget {
  final String apiBaseUrl = dotenv.get('API_BASE_URL');

  StreamPlayer({super.key});

  @override
  State<StreamPlayer> createState() => _WebRTCStreamPlayerState();
}

class _WebRTCStreamPlayerState extends State<StreamPlayer> {
  final RTCVideoRenderer _renderer = RTCVideoRenderer();
  RTCPeerConnection? _peerConnection;

  bool _isStreamReady = false;

  @override
  void initState() {
    super.initState();
    _initRenderer();
  }

  Future<void> _initRenderer() async {
    await _renderer.initialize();
    await _connect();

    _renderer.onResize = () {
      if (!_isStreamReady) {
        setState(() {
          _isStreamReady = true;
        });
      }
    };
  }

  Future<void> _connect() async {
    final httpClient = await getHttpClientWithCert();
    _peerConnection = await createPeerConnection({});

    // Hook up the video stream
    _peerConnection!.onTrack = (event) {
      // print('onTrack event received! Track kind: ${event.track.kind}');
      if (event.track.kind == 'video') {
        _renderer.srcObject = event.streams[0];
      }
    };

    // Transceiver setup
    await _peerConnection!.addTransceiver(
        kind: RTCRtpMediaType.RTCRtpMediaTypeVideo,
        init: RTCRtpTransceiverInit(direction: TransceiverDirection.RecvOnly));

    // Create Offer
    final offer = await _peerConnection!.createOffer();
    await _peerConnection!.setLocalDescription(offer);

    try {
      // Send to MediaMTX WHEP endpoint
      final response = await httpClient.post(
        Uri.parse('${widget.apiBaseUrl}:${dotenv.get('WEB_RTC_PORT')}/Vigi/whep'),
        headers: {'Content-Type': 'application/sdp'},
        body: offer.sdp,
      );

      // Handle Answer
      if (response.statusCode == 201) {
        final String answerSdp = response.body;
        await _peerConnection!.setRemoteDescription(
          RTCSessionDescription(answerSdp, 'answer'),
        );
      }
    }
    catch (exception) {
      print('MediaMTX WHEP POST Error: ${exception}');
    }
  }

  @override
  Widget build(BuildContext context) {
    const double padding = 16.0;
    final double screenWidth = MediaQuery.of(context).size.width;
    final double videoWidth = screenWidth - (padding * 2);

    return SizedBox(
        width: videoWidth,
        child: AspectRatio(
          aspectRatio: 16/9,
          child: ClipRRect(
            borderRadius: BorderRadius.circular(8.0),
            child: _isStreamReady ? Stack(
              children: [
                RTCVideoView(
                  _renderer,
                  objectFit: RTCVideoViewObjectFit.RTCVideoViewObjectFitContain,
                ),

                Positioned(
                  bottom: 8,
                  right: 8,
                  child: Container(
                    child: IconButton(
                      icon: const Icon(
                        Icons.fullscreen,
                        color: Colors.white,
                        size: 24,
                      ),
                      onPressed: () => _enterFullScreen(context),
                    )
                  )
                )
              ]
            ) : const Center(child: CircularProgressIndicator()),
          ),
        )
      );
  }

  void _enterFullScreen(BuildContext context) {
    Navigator.of(context).push(
      MaterialPageRoute(
        builder: (context) => FullScreenStreamPlayer(renderer: _renderer)
      )
    );
  }
}