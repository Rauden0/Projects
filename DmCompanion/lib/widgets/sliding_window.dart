import 'package:flutter/cupertino.dart';
import 'package:tes/widgets/sliding_tile.dart';

class SlidingWindow extends StatelessWidget {
  const SlidingWindow({super.key});

  @override
  Widget build(BuildContext context) {
    return ListView.builder(
      scrollDirection: Axis.horizontal,
      padding: EdgeInsets.all(2),
      itemBuilder: (context, index) {
        return SlidingTile();
      },
    );
  }
}
