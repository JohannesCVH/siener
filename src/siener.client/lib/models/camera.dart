class Camera {
  final String name;

  Camera({required this.name});

  factory Camera.fromJson(Map<String, dynamic> json) {
    return Camera(
      name: json['name'] as String,
    );
  }
}