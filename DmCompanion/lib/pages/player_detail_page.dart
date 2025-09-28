import 'package:flutter/material.dart';
import 'package:tes/widgets/page_template.dart';

import '../models/player.dart';

class PlayerDetailPage extends StatelessWidget {
  final Player player;

  const PlayerDetailPage({super.key, required this.player});

  @override
  Widget build(BuildContext context) {
    return PageTemplate(
      title: player.name,
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Player Image
            Center(
              child: CircleAvatar(
                backgroundImage: AssetImage(player.imagePath),
                radius: 50,
              ),
            ),
            const SizedBox(height: 16),

            // Player Basic Info
            Text(
              "Player Details",
              style: Theme.of(context).textTheme.headlineSmall,
            ),
            const SizedBox(height: 8),
            Text("ID: ${player.id}"),
            Text("Health: ${player.health}/${player.maxHealth}"),
            const SizedBox(height: 16),

            // Statistics
            Text(
              "Statistics",
              style: Theme.of(context).textTheme.headlineSmall,
            ),
            const SizedBox(height: 8),
            _buildStatRow("Total Damage Dealt", player.totalDamageDealt),
            _buildStatRow("Total Damage Taken", player.totalDamageTaken),
            _buildStatRow("Total Healing Done", player.totalHealingDone),
            _buildStatRow("Total Healing Received", player.totalHealingTaken),
          ],
        ),
      ),
    );
  }

  // Helper widget for statistics rows
  Widget _buildStatRow(String label, int value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4.0),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(label, style: const TextStyle(fontSize: 16)),
          Text(value.toString(), style: const TextStyle(fontSize: 16)),
        ],
      ),
    );
  }
}
