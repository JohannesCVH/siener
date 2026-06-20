import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter_webrtc/flutter_webrtc.dart';
import 'package:siener.mobile/http_client.dart';

class CameraScreen extends StatelessWidget {
  const CameraScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Camera'),
      ),
      body: Column(
        children: [
          Center(child: WebRTCStreamPlayer())
        ],
      ),
    );
  }
}

class WebRTCStreamPlayer extends StatefulWidget {
  final String apiBaseUrl = dotenv.get('API_BASE_URL');

  WebRTCStreamPlayer({super.key});

  @override
  State<WebRTCStreamPlayer> createState() => _WebRTCStreamPlayerState();
}

class _WebRTCStreamPlayerState extends State<WebRTCStreamPlayer> {
  final RTCVideoRenderer _renderer = RTCVideoRenderer();
  RTCPeerConnection? _peerConnection;

  @override
  void initState() {
    super.initState();
    _initRenderer();
  }

  Future<void> _initRenderer() async {
    await _renderer.initialize();
    await _connect();
  }

  Future<void> _connect() async {
    final httpClient = await getHttpClientWithCert();
    _peerConnection = await createPeerConnection({});

    // Hook up the video stream
    _peerConnection!.onTrack = (event) {
      // print('onTrack event received! Track kind: ${event.track.kind}');
      if (event.track.kind == 'video') {
        _renderer.srcObject = event.streams[0];
        setState(() {});
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
          child: RTCVideoView(
            _renderer,
            objectFit: RTCVideoViewObjectFit.RTCVideoViewObjectFitContain,
          ),
        )
      );
  }
}