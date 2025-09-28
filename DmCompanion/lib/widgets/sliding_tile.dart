import 'package:flutter/material.dart';

class SlidingTile extends StatelessWidget {
  SlidingTile({super.key});

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 100,
      width: 100,
      color: Colors.blue,
      child: Center(
        child: Text('Hello World'),
      ),
    );
  }
}
