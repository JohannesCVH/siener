import 'package:flutter/material.dart';
import 'package:siener.client/models/camera.dart';
import 'package:siener.client/widgets/stream_player_widget.dart';

class CameraScreen extends StatelessWidget {
  final Camera camera;
  
  const CameraScreen({
    super.key, 
    required this.camera
  });

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Camera'),
      ),
      body: Column(
        children: [
          Center(child: StreamPlayerWidget(camera: camera))
        ],
      ),
    );
  }
}