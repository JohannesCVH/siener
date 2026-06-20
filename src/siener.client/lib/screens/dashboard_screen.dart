import 'package:flutter/material.dart';
import 'package:siener.client/logger.dart';
import 'package:siener.client/models/camera.dart';
import 'package:siener.client/screens/camera_screen.dart';
import 'package:siener.client/services/camera_service.dart';
import 'package:siener.client/widgets/camera_card_widget.dart';

class DashboardScreen extends StatefulWidget {
  const DashboardScreen({super.key});
  
  @override
  State<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends State<DashboardScreen> {

  final CameraService _cameraService = CameraService();
  List<Camera>? _cameras;
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadCameras();
  }

  Future<void> _loadCameras() async {
    const String functionName = '_loadCameras';
    
    _isLoading = true;
    final List<Camera>? cameras = await _cameraService.fetchCameras();
    
    if (cameras != null && cameras.isNotEmpty) {
      setState(() {
        _cameras = cameras;

        //Temp testing
        final template = cameras.first;
        _cameras?.addAll(List.generate(11, (index) => Camera(name: '${template.name}_$index')));
        
        _cameras?.forEach((camera) => logMessage(_DashboardScreenState, functionName, 'Camera loaded: ${camera.name}'));
        _isLoading = false;
      });
    } else {
      setState(() {
        _isLoading = false;
      });
    }
  }
  
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Dashboard')),        
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : (_cameras == null || _cameras!.isEmpty)
              ? const Center(child: Text('No cameras available'))
              : GridView.builder(
                  padding: const EdgeInsets.all(8.0),
                  gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                    crossAxisCount: 2,
                    childAspectRatio: 1.0,
                    crossAxisSpacing: 8.0,
                    mainAxisSpacing: 8.0,
                  ),
                  itemCount: _cameras!.length,
                  itemBuilder: (context, index) {
                    final camera = _cameras![index];
                    return GestureDetector(
                      onTap: () {
                        Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (context) => const CameraScreen(),
                          ),
                        );
                      },
                      child: CameraCardWidget(
                        camera: camera,
                        cameraService: _cameraService,
                      ),
                    );
                  },
                ),
    );
  }
}