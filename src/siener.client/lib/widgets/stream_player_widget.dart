import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_webrtc/flutter_webrtc.dart';
import 'package:sensors_plus/sensors_plus.dart';
import 'package:siener.client/models/camera.dart';
import 'package:siener.client/services/stream_player_service.dart';
import 'package:siener.client/widgets/fullscreen_stream_player_widget.dart';

class StreamPlayerWidget extends StatefulWidget {
  final Camera camera;

  StreamPlayerWidget({
    super.key,
    required this.camera
  });

  @override
  State<StreamPlayerWidget> createState() => _WebRTCStreamPlayerState();
}

class _WebRTCStreamPlayerState extends State<StreamPlayerWidget> {
  late final StreamPlayerService _streamPlayerService;
  StreamSubscription<AccelerometerEvent>? _accelerometerSubscription;

  @override
  void initState() {
    super.initState();
    
    _streamPlayerService = StreamPlayerService(camera: widget.camera);
    _initService();

    _accelerometerSubscription = accelerometerEventStream().listen((event) {
      //If Landscape
      if (event.x.abs() > 7.0 && event.y.abs() < 4.0) {
        if (ModalRoute.of(context)?.isCurrent == true && _streamPlayerService.isRotateable) {
          _enterFullScreen(context);
        }
      }
      
      //If Portrait
      if (event.y.abs() > 7.0 && event.x.abs() < 4.0) {
        _streamPlayerService.isRotateable = true;
      }
    });
  }

  Future<void> _initService() async {
    await _streamPlayerService.initialize();

    _streamPlayerService.renderer.onResize = () {
      if (!_streamPlayerService.isStreamReady) {
        setState(() {
          _streamPlayerService.isStreamReady = true;
        });
      }
    };
  }

  @override
  Widget build(BuildContext context) {
    const double padding = 16.0;
    final double screenWidth = MediaQuery.of(context).size.width;
    final double videoWidth = screenWidth - (padding * 2);

    return OrientationBuilder(
      builder: (context, orientation) {
        return SizedBox(
          width: videoWidth,
          child: AspectRatio(
            aspectRatio: 16/9,
            child: ClipRRect(
              borderRadius: BorderRadius.circular(8.0),
              child: _streamPlayerService.isStreamReady ? Stack(
                children: [
                  RTCVideoView(
                    _streamPlayerService.renderer,
                    objectFit: RTCVideoViewObjectFit.RTCVideoViewObjectFitContain,
                  ),
                ]
              ) : const Center(child: CircularProgressIndicator()),
            ),
          )
        );
      }
    );
  }

  void _enterFullScreen(BuildContext context) async {
    _streamPlayerService.isRotateable = false;
    await Navigator.of(context).push(
      MaterialPageRoute(
        builder: (context) => FullScreenStreamPlayerWidget(streamPlayerService: _streamPlayerService,)
      )
    );

    SystemChrome.setPreferredOrientations([
      DeviceOrientation.portraitUp,
    ]);
  }

  @override
  void dispose() {
    _accelerometerSubscription?.cancel();
    _streamPlayerService.dispose();
    super.dispose();
  }
}